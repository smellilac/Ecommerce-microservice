using Ecommerce.ProductService.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Ecommerce.ProductService.Kafka.Consumer;

public class HostedKafkaConsumer : BackgroundService, IDisposable
{
    private readonly KafkaConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDistributedCache _cache;
    private readonly ILogger<HostedKafkaConsumer> _logger;

    public HostedKafkaConsumer(
        IServiceScopeFactory scopeFactory,
        IDistributedCache cache,
        KafkaConsumer consumer,
        ILogger<HostedKafkaConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var activity = new Activity("ConsumeKafkaMessage"))
            {
                activity.Start();  

                try
                {
                    _logger.LogInformation("Waiting for Kafka message...");

                    _logger.LogInformation("Attempting to consume message from Kafka...");
                    var order = await _consumer.ConsumeAsync(stoppingToken);

                    if (order == null)
                    {
                        _logger.LogInformation("No order message received from Kafka.");
                        activity.SetTag("consumeResult", "null");
                        continue;
                    }

                    _logger.LogInformation($"Received order {order}");
                    activity.SetTag("orderRequestId", order.RequestId);
                    activity.SetTag("orderProductId", order.ProductId);

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                    // Логирование поиска продукта в базе данных
                    activity.AddEvent(new ActivityEvent("SearchingProductInDb"));
                    var product = await dbContext.Products.FindAsync(order.ProductId);

                    if (product == null)
                    {
                        _logger.LogWarning("Product {product} not found in the database.", product);
                        activity.SetTag("productFound", "false");
                        continue;
                    }

                    activity.SetTag("productFound", "true");

                    var cacheKey = $"RequestId:{order.RequestId}";
                    _logger.LogInformation($"Checking cache for RequestId: {order.RequestId}...");
                    activity.AddEvent(new ActivityEvent("CheckingCache"));
                    var processed = await _cache.GetStringAsync(cacheKey);

                    if (!string.IsNullOrEmpty(processed))
                    {
                        _logger.LogInformation($"Order with RequestId: {order.RequestId} already processed. Skipping.");
                        activity.SetTag("orderProcessed", "true");
                        continue;
                    }

                    _logger.LogInformation("Updating product {product}", product);
                    activity.AddEvent(new ActivityEvent("UpdatingProduct"));
                    product.Quantity -= order.Quantity;
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Product new Quantity: {product.Quantity}");

                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    };

                    _logger.LogInformation($"Updating cache with processed flag for RequestId: {order.RequestId}");
                    activity.AddEvent(new ActivityEvent("UpdatingCache"));
                    await _cache.SetStringAsync(cacheKey, "processed", options);

                    _logger.LogInformation($"Cache updated with processed flag for RequestId: {order.RequestId}");
                    activity.SetTag("cacheUpdated", "true");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka Consumer Error");
                    activity.SetTag("error", true);
                    activity.SetTag("errorMessage", ex.Message);
                }
                finally
                {
                    activity.Stop();
                }
            }
        }
    }
}
