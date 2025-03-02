using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ecommerce.OrderService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class OrderController(OrderDbContext context, IKafkaProducer producer) : ControllerBase
{
    private readonly OrderDbContext _context = context;
    private readonly IKafkaProducer _producer = producer;

    [HttpGet]
    public async Task<List<OrderModel>> GetOrders()
         => await _context.Orders.ToListAsync();

    [HttpPost]
    public async Task<OrderModel> CreateOrder(OrderModel order)
    {
        order.OrderDate = DateTime.UtcNow;
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        await _producer.ProduceAsync("order-topic", new Message<string, string>
        {
            Key = order.Id.ToString(),
            Value = JsonSerializer.Serialize(order)
        });

        return order;
    }
}
