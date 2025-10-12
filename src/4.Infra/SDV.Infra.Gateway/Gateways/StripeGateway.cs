using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Gateway.Gateways;

public class StripeGateway : IPaymentGateway
{
    public Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request)
    {
        throw new NotImplementedException("Stripe ainda não foi implementado.");
    }

    public Task<Result<string>> GetPaymentExternalReferenceAsync(string paymentId)
    {
        throw new NotImplementedException("Stripe ainda não foi implementado.");
    }

    public Task<Result<PaymentStatus>> GetPaymentStatusAsync(string paymentId)
    {
        throw new NotImplementedException("Stripe ainda não foi implementado.");
    }

    public Result<bool> ValidateWebhookSecret(string secret)
    {
        throw new NotImplementedException("Stripe ainda não foi implementado.");
    }
}
