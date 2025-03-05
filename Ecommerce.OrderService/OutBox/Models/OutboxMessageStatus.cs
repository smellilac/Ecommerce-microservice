namespace Ecommerce.OrderService.OutBox.Models;

public enum OutboxMessageStatus
{
    Success = 0,
    InProgress = 1,
    Canceled = 2
}