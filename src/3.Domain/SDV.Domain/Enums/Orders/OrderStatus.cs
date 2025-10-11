namespace SDV.Domain.Enums.Orders;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    PaymentFailed
}