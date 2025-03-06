using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using MediatR;

namespace Ecommerce.ProductService.Query;

public class GetProductByIdQueryHandler(ProductDbContext context) : IRequestHandler<GetProductByIdQuery, ProductModel>
{
    private readonly ProductDbContext _context = context;
    public async Task<ProductModel> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        => await _context.Products.FindAsync(request.Id, cancellationToken);
}
