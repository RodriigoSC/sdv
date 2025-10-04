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
        
        // Primeiro, cria o pagamento para obter a referência externa
        var paymentResult = await _paymentGateway.CreatePaymentAsync(subscription, plan);

        if (!paymentResult.IsSuccess)
        {
            return Result<Subscription>.Failure(paymentResult.Error ?? "Falha ao criar o pagamento.");
        }

        // Agora que temos o resultado, salvamos a assinatura
        await _subscriptionRepository.AddAsync(subscription);
        
        // Atribuímos a URL de pagamento ao ID da transação para retornar ao front-end
        subscription.SetTransactionId(paymentResult.Value ?? string.Empty);

        return Result<Subscription>.Success(subscription);
    }

    public async Task<Result<Subscription>> GetSubscriptionByClientAsync(Guid clientId)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync();
        var subscription = subscriptions.FirstOrDefault(s => s.ClientId == clientId);
        
        return subscription != null 
            ? Result<Subscription>.Success(subscription)
            : Result<Subscription>.Failure("Assinatura não encontrada.");
    }

    public async Task<Result<bool>> ProcessPaymentCallbackAsync(string paymentId)
    {
        // 1. Obter o status
        var paymentStatusResult = await _paymentGateway.GetPaymentStatusAsync(paymentId);
        if (!paymentStatusResult.IsSuccess)
        {
            return Result<bool>.Failure(paymentStatusResult.Error ?? "Não foi possível obter o status do pagamento.");

        }

        // 2. Obter a referência externa (ID da assinatura)
        var externalRefResult = await _paymentGateway.GetPaymentExternalReferenceAsync(paymentId);
        if (!externalRefResult.IsSuccess)
        {
            return Result<bool>.Failure(externalRefResult.Error ?? "Não foi possível obter a referência externa do pagamento.");

        }

        if (!Guid.TryParse(externalRefResult.Value, out var subscriptionId))
        {
            return Result<bool>.Failure("Referência externa inválida.");
        }

        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
        if (subscription == null)
        {
            return Result<bool>.Failure("Assinatura não encontrada.");
        }

        // 3. Atualizar o status da assinatura
        switch (paymentStatusResult.Value)
        {
            case Domain.Enums.Payments.PaymentStatus.Approved:
                subscription.Activate();
                subscription.SetEndDate(DateTime.UtcNow.AddYears(1)); // Exemplo para plano anual
                break;
            case Domain.Enums.Payments.PaymentStatus.Refused:
                subscription.Fail();
                break;
        }

        await _subscriptionRepository.UpdateAsync(subscription);
        return Result<bool>.Success(true);
    }

}
