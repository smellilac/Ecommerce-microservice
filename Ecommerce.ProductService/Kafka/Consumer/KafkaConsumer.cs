using System.Diagnostics;
using Confluent.Kafka;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Model;

public class KafkaConsumer : IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private bool _disposed = false;

    public KafkaConsumer(ILogger<KafkaConsumer> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            GroupId = "order-group",
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe("order-topic");
    }

    public async Task<OrderModel?> ConsumeAsync(CancellationToken stoppingToken)
    {
        using (var activity = new Activity("KafkaConsume"))
        {
            activity.Start();

            try
            {
                _logger.LogInformation("Starting to consume messages.");

                var consumeResult = await Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken);

                if (consumeResult?.Message?.Value == null)
                {
                    _logger.LogWarning("Message value is null.");
                    activity.SetTag("consumeResult", "null");
                    activity.Stop();
                    return null;
                }

                var order = JsonSerializer.Deserialize<OrderModel>(consumeResult.Message.Value);

                activity.SetTag("messageKey", consumeResult.Message.Key);

                activity.Stop();
                return order;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka Consume Error: {ErrorReason}", ex.Error.Reason);
                activity.SetTag("error", true);
                activity.SetTag("errorMessage", ex.Error.Reason);
                activity.Stop();
                Console.WriteLine($"Kafka Consume Error: {ex.Error.Reason}");
                return null;
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "Consumer has been disposed with errror: {Message}", ex.Message);
                activity.SetTag("error", true);
                activity.SetTag("errorMessage", ex.Message);
                activity.Stop();
                Console.WriteLine($"Consumer has been disposed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error: {Message}", ex.Message);
                activity.SetTag("error", true);
                activity.SetTag("errorMessage", ex.Message);
                activity.Stop();
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return null;
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _consumer.Unsubscribe();
            _consumer.Close();
            _consumer.Dispose();
            _disposed = true;
        }
    }
}
