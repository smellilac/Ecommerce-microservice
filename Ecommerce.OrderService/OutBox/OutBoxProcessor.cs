using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Ecommerce.OrderService.OutBox.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;

public class OutBoxProcessor(IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var _producer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

            var messages = await context.OutboxOrders
                .Where(m => m.Status == OutboxMessageStatus.InProgress && m.ResendTime <= DateTime.UtcNow.AddMinutes(1))
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
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    message.ResendTime = DateTime.UtcNow.AddSeconds(40);
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            await CleanUpProcessedMessagesAsync(context, stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);  
        }
    }

    private async Task CleanUpProcessedMessagesAsync(OrderDbContext context, CancellationToken stoppingToken)
    {
        var connectionString = _configuration.GetConnectionString("OrderConnection");

        var sqlQueryForDelete = "Delete From \"OutboxOrders\" Where \"Status\" = @status";

        using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(stoppingToken);

            using (var command = new NpgsqlCommand(sqlQueryForDelete, connection))
            {
                var status = (int)OutboxMessageStatus.Success;
                command.Parameters.AddWithValue("@status", status);
                var rowsAffected = await command.ExecuteNonQueryAsync();
            }
        }
    }
}
