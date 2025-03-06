using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Ecommerce.OrderService.OutBox.Models;

namespace Ecommerce.OrderService.OutBox;

public class OutBoxProcessor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var _producer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

        var messages = await context.OutboxOrders.
             Where(m => m.Status == OutboxMessageStatus.InProgress && m.ResendTime <= DateTime.UtcNow)
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
