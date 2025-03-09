namespace Ecommerce.Model;

public class OrderModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string CustomerName { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
    public Guid RequestId { get; set; }

    public override string ToString()
    {
        return $"Order (Id: {Id}, ProductId: {ProductId}, Customer: {CustomerName}, Quantity: {Quantity}, OrderDate: {OrderDate}, RequestId: {RequestId})";
    }
}
