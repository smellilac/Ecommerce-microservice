namespace Ecommerce.OrderService.Commands.CreateOrder;

using Ecommerce.Model;
using MediatR;
public record CreateOrderCommand(int ProductId, string CustomerName, int Quantity) : IRequest<OrderModel>;
