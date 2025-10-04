using SDV.Application.Dtos.Payments;
using SDV.Application.Dtos.Subscriptions;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

public interface ISubscriptionApplication
{
    Task<OperationResult<CreatePaymentDto>> Subscribe(string planId, string clientId);
    Task<OperationResult<bool>> ProcessPaymentCallback(string paymentId, string secret);
    Task<OperationResult<SubscriptionDto>> GetSubscriptionByClientId(string clientId);
}
