using Ecommerce.Model;
using Ecommerce.OrderService.OutBox.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.OrderService.Data;

public class OrderDbContext : DbContext
{
    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OutboxOrderMessage> OutboxOrders { get; set; }
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) 
    {
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxOrderMessage>()
            .HasKey(o => o.MessageId); 
    }
}
