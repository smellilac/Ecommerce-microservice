using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.ProductService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class ProductController(ProductDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<List<ProductModel>> GetProducts()
        => await context.Products.ToListAsync();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) 
            return NotFound();

        return Ok(product);
    }
}
