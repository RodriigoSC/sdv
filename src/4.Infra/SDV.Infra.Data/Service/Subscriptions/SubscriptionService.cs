using SDV.Domain.Entities.Clients;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Plans;
using SDV.Domain.Enums.Subscriptions;
using SDV.Domain.Interfaces.Payments;
using SDV.Domain.Interfaces.Plans;
using SDV.Domain.Interfaces.Subscriptions;

namespace SDV.Infra.Data.Service.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IPaymentGateway _paymentGateway;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IPlanRepository planRepository, IPaymentGateway paymentGateway)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result<Subscription>> CreateSubscriptionAsync(Client client, Plan plan)
    {
        // Validação: Cliente já possui assinatura ativa?
        var existingSubscriptions = await _subscriptionRepository.GetAllAsync();
        var activeSubscription = existingSubscriptions.FirstOrDefault(s => 
            s.ClientId == client.Id && 
            (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Pending));

        if (activeSubscription != null)
        {
            return Result<Subscription>.Failure("Cliente já possui uma assinatura ativa ou pendente.");
        }

        var subscription = new Subscription(client.Id, plan.Id);
        
        // Cria o pagamento no Mercado Pago
        var paymentResult = await _paymentGateway.CreatePaymentAsync(subscription, plan);

        if (!paymentResult.IsSuccess)
        {
            return Result<Subscription>.Failure(paymentResult.Error ?? "Falha ao criar o pagamento.");
        }

        // Atribui a URL de pagamento ao TransactionId ANTES de salvar
        subscription.SetTransactionId(paymentResult.Value ?? string.Empty);

        // Salva a assinatura com o TransactionId já definido
        await _subscriptionRepository.AddAsync(subscription);

        return Result<Subscription>.Success(subscription);
    }

    public async Task<Result<Subscription>> GetSubscriptionByClientAsync(Guid clientId)
    {
        var subscriptions = await _subscriptionRepository.GetAllAsync();
        var subscription = subscriptions
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault();
        
        return subscription != null 
            ? Result<Subscription>.Success(subscription)
            : Result<Subscription>.Failure("Assinatura não encontrada.");
    }

    public async Task<Result<bool>> ProcessPaymentCallbackAsync(string paymentId)
    {
        // 1. Obter o status do pagamento
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

        if (subscription.LastProcessedPaymentId == paymentId)
        {
            return Result<bool>.Success(true);
        }

        // 3. Buscar o plano para calcular a duração corretamente
        var plan = await _planRepository.GetByIdAsync(subscription.PlanId);

        // 4. Atualizar o status da assinatura baseado no status do pagamento
        switch (paymentStatusResult.Value)
        {
            case Domain.Enums.Payments.PaymentStatus.Approved:
                subscription.Activate();
                
                // Calcula a duração baseada no tipo de plano
                if (plan != null)
                {
                    var endDate = plan.CalculateEndDate(plan.PlanType);
                    subscription.SetEndDate(endDate);
                }
                else
                {
                    // Fallback: 1 mês se não conseguir buscar o plano
                    subscription.SetEndDate(DateTime.UtcNow.AddMonths(1));
                }
                break;

            case Domain.Enums.Payments.PaymentStatus.Refused:
                subscription.Fail();
                break;

            case Domain.Enums.Payments.PaymentStatus.Pending:
                // Mantém como Pending, não faz nada
                break;
        }
        
        subscription.SetLastProcessedPayment(paymentId);
        await _subscriptionRepository.UpdateAsync(subscription);
        
        return Result<bool>.Success(true);
    }
}
