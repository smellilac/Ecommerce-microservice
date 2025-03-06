using Ecommerce.Model;
using MediatR;

namespace Ecommerce.ProductService.Query;

public class GetProductByIdQuery(int id) : IRequest<ProductModel>
{
    public int Id { get; set; } = id;
}
