using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Payment;

public class MercadoPagoGateway : IPaymentGateway
{
    public Task<Result<string>> CreatePaymentAsync(Subscription subscription, Plan plan)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PaymentStatus>> GetPaymentStatusAsync(string transactionId)
    {
        throw new NotImplementedException();
    }
}
