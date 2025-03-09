namespace Ecommerce.Model;

public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public override string ToString()
    {
        return $"Product (Id: {Id}, Name: {Name}, Price: {Price}, Quantity: {Quantity})";
    }
}
