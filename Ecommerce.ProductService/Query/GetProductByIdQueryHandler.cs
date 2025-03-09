using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using MediatR;

namespace Ecommerce.ProductService.Query;

public class GetProductByIdQueryHandler(ProductDbContext context, ILogger<GetProductByIdQueryHandler> logger) : IRequestHandler<GetProductByIdQuery, ProductModel>
{
    private readonly ProductDbContext _context = context;
    private readonly ILogger<GetProductByIdQueryHandler> _logger = logger;
    public async Task<ProductModel> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.Id, cancellationToken);
        if (product is null)
            _logger.Log(LogLevel.Information, "Product: {@request} not found", request);

        _logger.Log(LogLevel.Information, "Product: {@request} successfuly found", request);
        return product!;
    }
}
