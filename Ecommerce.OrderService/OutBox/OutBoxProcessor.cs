using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ecommerce.OrderService.OutBox;

public class OutBoxProcessor(IServiceScopeFactory scopeFactory, IKafkaProducer producer) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IKafkaProducer _producer = producer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var messages = await context.OutboxOrders.
             Where(m => m.Status == Model.OutboxMessageStatus.InProgress && m.ResendTime <= DateTime.UtcNow)
             .ToListAsync(stoppingToken);

        foreach (var message in messages)
        {
            try
            {
                var order = JsonSerializer.Deserialize<OrderModel>(message.Payload);
                await _producer.ProduceAsync("order-topic", new Message<string, string>
                {
                    Key = order.Id.ToString(),
                    Value = message.Payload
                });

                message.Status = OutboxMessageStatus.Success;
                await context.SaveChangesAsync(stoppingToken); // 
            }
            catch
            {
                message.ResendTime = DateTime.UtcNow.AddSeconds(40);
                await context.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);
        }
    }
}
