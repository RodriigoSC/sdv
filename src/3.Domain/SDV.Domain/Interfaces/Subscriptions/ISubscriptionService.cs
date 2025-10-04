using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;

namespace SDV.Domain.Interfaces.Subscriptions;

public interface ISubscriptionService
{
    Task<Result<Subscription>> CreateSubscriptionAsync(Client client, Plan plan);
    Task<Result<Subscription>> GetSubscriptionByClientAsync(Guid clientId);
    Task<Result<bool>> ProcessPaymentCallbackAsync(string transactionId, string status);
}
