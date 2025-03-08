using Confluent.Kafka;

namespace Ecommerce.OrderService.Kafka.Producer;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private bool _disposed = false;

    public KafkaProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    public async Task ProduceAsync(string topic, Message<string, string> message)
    {
        try
        {
            var deliveryResult = await _producer.ProduceAsync(topic, message);
        }
        catch (ProduceException<string, string> ex)
        {
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _producer.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        // освобождаем неуправляемые ресурсы
        Dispose(true);
        // подавляем финализацию
        GC.SuppressFinalize(this);
    } // microsoft recomendation https://metanit.com/sharp/tutorial/8.2.php
}
