namespace Ecommerce.OrderService.Commands.CreateOrder;

using Ecommerce.Model;
using MediatR;
public class CreateOrderCommand : IRequest<OrderModel>
{
    public int ProductId { get; set; }
    public string CustomerName { get; set; }
    public int Quantity { get; set; }
}

