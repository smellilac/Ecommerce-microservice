namespace Ecommerce.Model;

public enum OutboxMessageStatus
{
    Success = 0,
    InProgress = 1,
    Canceled = 2
}
