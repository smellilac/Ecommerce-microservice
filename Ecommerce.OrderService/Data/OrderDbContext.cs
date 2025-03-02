using Ecommerce.Model;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Data;

public class OrderDbContext : DbContext
{
    public DbSet<OrderModel> Orders { get; set; }
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) 
    {
        Database.EnsureCreated(); 
    } // switch for migrations 
}
