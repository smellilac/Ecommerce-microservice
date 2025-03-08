using Confluent.Kafka;
using Ecommerce.Model;
using System.Text.Json;

namespace Ecommerce.ProductService.Kafka.Consumer;

public class KafkaConsumer 
{
    private readonly IConsumer<string, string> _consumer;
    private bool _disposed = false;

    public KafkaConsumer()
    {
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
        try
        {
            var consumeResult = await Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken);

            if (consumeResult?.Message?.Value == null)
                return null;

            return JsonSerializer.Deserialize<OrderModel>(consumeResult.Message.Value);
        }
        catch (ConsumeException ex)
        {
            Console.WriteLine($"Kafka Consume Error: {ex.Error.Reason}");
            return null;
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine($"Consumer has been disposed: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            return null;
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
