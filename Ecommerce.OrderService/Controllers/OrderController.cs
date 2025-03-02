using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class OrderController(OrderDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<List<OrderModel>> GetOrders()
         => await context.Orders.ToListAsync();

    [HttpPost]
    public async Task<OrderModel> CreateOrder(OrderModel order)
    {
        order.OrderDate = DateTime.UtcNow;
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        return order;
    }
}
