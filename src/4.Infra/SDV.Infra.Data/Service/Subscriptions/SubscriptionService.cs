using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Interfaces.Payments;
using SDV.Domain.Interfaces.Subscriptions;

namespace SDV.Infra.Data.Service.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPaymentGateway _paymentGateway;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IPaymentGateway paymentGateway)
        {
            _subscriptionRepository = subscriptionRepository;
            _paymentGateway = paymentGateway;
        }

        public async Task<Result<Subscription>> CreateSubscriptionAsync(Client client, Plan plan)
        {
            var subscription = new Subscription(client.Id, plan.Id);
            await _subscriptionRepository.AddAsync(subscription);

            var paymentResult = await _paymentGateway.CreatePaymentAsync(subscription, plan);

            if (!paymentResult.IsSuccess)
            {
                return Result<Subscription>.Failure(paymentResult.Error);
            }

            return Result<Subscription>.Success(subscription);
        }

        public async Task<Result<Subscription>> GetSubscriptionByClientAsync(Guid clientId)
        {
            // Implementar a busca no repositório
            throw new NotImplementedException();
        }

        public async Task<Result<bool>> ProcessPaymentCallbackAsync(string transactionId, string status)
        {
            // Implementar a lógica de callback
            throw new NotImplementedException();
        }

}
