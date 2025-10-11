using SDV.Application.Dtos.Orders;
using SDV.Application.Dtos.Payments;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface IOrderApplication
{
    Task<OperationResult<CreatePaymentDto>> Subscribe(string planId, string clientId);
    Task<OperationResult<bool>> ProcessPaymentCallback(string paymentId, string secret);
    Task<OperationResult<OrderDto>> GetSubscriptionByClientId(string clientId);
}
