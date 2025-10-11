using Microsoft.Extensions.DependencyInjection;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Payment.Gateways;

namespace SDV.Infra.Payment.Factories;

public class PaymentProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGateway GetProvider(PaymentProvider provider)
    {
        return provider switch
        {
            PaymentProvider.MercadoPago => _serviceProvider.GetRequiredService<MercadoPagoGateway>(),
            PaymentProvider.PagarMe => _serviceProvider.GetRequiredService<PagarMeGateway>(),
            PaymentProvider.Stripe => _serviceProvider.GetRequiredService<StripeGateway>(),
            _ => throw new NotSupportedException($"Provedor de pagamento '{provider}' não suportado.")
        };
    }
}