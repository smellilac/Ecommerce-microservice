using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.ProductService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class ProductController(ProductDbContext context, IMediator mediator) : ControllerBase
{
    private readonly ProductDbContext _context = context;
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<List<ProductModel>> GetProducts()
        => await _context.Products.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ProductModel> GetProductById(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return product;
    }
}
