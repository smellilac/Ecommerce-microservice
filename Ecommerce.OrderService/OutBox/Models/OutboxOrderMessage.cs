namespace Ecommerce.OrderService.OutBox.Models;

public class OutboxOrderMessage
{
    public int MessageId { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public string Payload { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ResendTime { get; set; }
    public string Type { get; set; }
}
