using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Commands.CreateOrder;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Ecommerce.OrderService.OutBox.Models;
using MediatR;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

public class CreateOrderCommandHandler(OrderDbContext context) : IRequestHandler<CreateOrderCommand, OrderModel>
{
    private readonly OrderDbContext _context = context;

    public async Task<OrderModel> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = new OrderModel
            {
                Quantity = request.Quantity,
                ProductId = request.ProductId,
                CustomerName = request.CustomerName,
                OrderDate = DateTime.UtcNow,
                RequestId = Guid.NewGuid()
            };

            await _context.Orders.AddAsync(order, cancellationToken);

            var outboxMessage = new OutboxOrderMessage
            {
                Status = OutboxMessageStatus.InProgress, 
                Payload = JsonSerializer.Serialize(order),
                CreatedDate = DateTime.UtcNow,
                ResendTime = DateTime.UtcNow,
                Type = "OrderCreated"
            };

            await _context.OutboxOrders.AddAsync(outboxMessage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return order;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
