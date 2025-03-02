using Confluent.Kafka;

namespace Ecommerce.OrderService.Kafka.Producer;

public interface IKafkaProducer
{
    Task ProduceAsync(string topic, Message<string, string> message);
}
