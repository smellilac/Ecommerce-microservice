using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Ecommerce.OrderService.OutBox.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;
using System.Text.Json;

public class OutBoxProcessor(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<OutBoxProcessor> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<OutBoxProcessor> _logger = logger;

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
                using var activity = new Activity("Processing Outbox Message").Start();
                activity?.SetTag("message.id", message.MessageId);
                activity?.SetTag("message.status", message.Status.ToString());

                try
                {
                    _logger.LogInformation("Processing message {MessageId}", message.MessageId);
                    var order = JsonSerializer.Deserialize<OrderModel>(message.Payload);
                    await _producer.ProduceAsync("order-topic", new Message<string, string>
                    {
                        Key = order.Id.ToString(),
                        Value = message.Payload
                    });

                    message.Status = OutboxMessageStatus.Success;
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Message {MessageId} processed successfully", message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
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
                _logger.LogInformation("Cleaned up {RowsAffected} processed messages", rowsAffected);
            }
        }
    }
}