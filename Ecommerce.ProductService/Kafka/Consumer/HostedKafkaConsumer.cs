using Ecommerce.ProductService.Data;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

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
            try
            {
                _logger.LogInformation("Waiting for Kafka message...");

                // Log before consuming a message from Kafka
                _logger.LogInformation("Attempting to consume message from Kafka...");
                var order = await _consumer.ConsumeAsync(stoppingToken);

                if (order == null)
                {
                    _logger.LogInformation("No order message received from Kafka.");
                    continue;
                }

                _logger.LogInformation($"Received order with RequestId: {order.RequestId} and ProductId: {order.ProductId}");

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                _logger.LogInformation($"Searching for Product with ProductId: {order.ProductId} in the database...");
                var product = await dbContext.Products.FindAsync(order.ProductId);

                if (product == null)
                {
                    _logger.LogWarning($"Product with ProductId: {order.ProductId} not found in the database.");
                    continue;
                }

                var cacheKey = $"RequestId:{order.RequestId}";
                _logger.LogInformation($"Checking cache for RequestId: {order.RequestId}...");
                var processed = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(processed))
                {
                    _logger.LogInformation($"Order with RequestId: {order.RequestId} already processed. Skipping.");
                    continue;
                }

                _logger.LogInformation($"Updating product with ProductId: {order.ProductId}. Current Quantity: {product.Quantity}");
                product.Quantity -= order.Quantity;
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Product with ProductId: {order.ProductId} updated. New Quantity: {product.Quantity}");

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                _logger.LogInformation($"Updating cache with processed flag for RequestId: {order.RequestId}");
                await _cache.SetStringAsync(cacheKey, "processed", options);

                _logger.LogInformation($"Cache updated with processed flag for RequestId: {order.RequestId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kafka Consumer Error: {ex.Message}");
                _logger.LogError(ex, "Stack trace of the exception:");
            }
        }
    }
}
