using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Orders;

namespace SDV.Domain.Interfaces.Orders;

public interface IOrderService
{
    Task<Result<Order>> CreateOrderAsync(Guid clientId, Guid planId);
    Task<Result<Order>> GetLastOrderByClientAsync(Guid clientId);
    Task<Result<bool>> ProcessPaymentCallbackAsync(Guid paymentId, string webhookSecret);
}
