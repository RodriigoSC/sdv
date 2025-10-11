using System;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;

using SDV.Infra.Payment.Factories;

namespace SDV.Infra.Data.Service.Payments;

public class PaymentService : IPaymentService
{
    private readonly PaymentProviderFactory _paymentProviderFactory;

    public PaymentService(PaymentProviderFactory paymentProviderFactory)
    {
        _paymentProviderFactory = paymentProviderFactory;
    }

    public async Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request)
    {
        try
        {
            var gateway = _paymentProviderFactory.GetProvider(request.PaymentProvider);

            if (gateway == null)
            {
                return Result<string>.Failure($"Gateway de pagamento '{request.PaymentProvider}' não suportado.");
            }

            var checkoutUrl = await gateway.CreatePaymentAsync(request);

            return checkoutUrl.IsSuccess
                ? Result<string>.Success(checkoutUrl.Value)
                : Result<string>.Failure(checkoutUrl.Error ?? "Erro ao criar pagamento");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erro ao criar pagamento: {ex.Message}");
        }
    }

    public Task<Result<Domain.Entities.Payments.Payment>> ProcessPaymentAsync(Domain.Entities.Payments.Payment payment)
    {
        throw new NotImplementedException();
    }
}
