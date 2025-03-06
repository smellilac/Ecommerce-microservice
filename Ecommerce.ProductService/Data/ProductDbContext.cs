using Ecommerce.Model;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.ProductService.Data;

public class ProductDbContext : DbContext
{
    public DbSet<ProductModel> Products { get; set; }
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) 
    { 
    }
}
