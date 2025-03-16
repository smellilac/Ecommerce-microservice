namespace Ecommerce.OrderService.Kafka.Producer;

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private static readonly ActivitySource ActivitySource = new("KafkaProducer");
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;
    private bool _disposed = false;

    public KafkaProducer(ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, Message<string, string> message)
    {
        using var activity = ActivitySource.StartActivity("Produce Message", ActivityKind.Producer);

        if (activity != null)
        {
            activity.SetTag("messaging.system", "kafka");
            activity.SetTag("messaging.destination", topic);
        }

        try
        {
            _logger.LogInformation("Producing message to topic {Topic}", topic);
            var deliveryResult = await _producer.ProduceAsync(topic, message);
            _logger.LogInformation("Message delivered to {TopicPartitionOffset}", deliveryResult.TopicPartitionOffset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error producing message to topic {Topic}", topic);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
