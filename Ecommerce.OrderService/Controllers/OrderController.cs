using Ecommerce.Model;
using Ecommerce.OrderService.Commands.CreateOrder;
using Ecommerce.OrderService.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(OrderDbContext context, IMediator mediator, ILogger<OrderController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly OrderDbContext _context = context;
    private readonly ILogger<OrderController> _logger = logger;

    [HttpGet]
    public async Task<List<OrderModel>> GetOrders()
        => await _context.Orders.ToListAsync();


    [HttpPost]
    public async Task<OrderModel> CreateOrder(CreateOrderCommand order)
        => await _mediator.Send(order);
}
