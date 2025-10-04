using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;

namespace SDV.Infra.Payment;

public class MercadoPagoGateway : IPaymentGateway
{
    private readonly string _accessToken;

    public MercadoPagoGateway(string accessToken)
    {
        _accessToken = accessToken;
        MercadoPagoConfig.AccessToken = _accessToken;
    }

    public async Task<Result<string>> CreatePaymentAsync(Subscription subscription, Plan plan)
    {
        try
        {
            var request = new PreferenceRequest
            {
                ExternalReference = subscription.Id.ToString(),
                Items = new List<PreferenceItemRequest>
                    {
                        new PreferenceItemRequest
                        {
                            Title = plan.Name,
                            Description = plan.Description,
                            Quantity = 1,
                            CurrencyId = "BRL",
                            UnitPrice = plan.Price,
                        },
                    },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "http://localhost:4200/subscribe/success", // Altere para sua URL de front-end
                    Failure = "http://localhost:4200/subscribe/failure",
                    Pending = "http://localhost:4200/subscribe/pending",
                },
                AutoReturn = "approved",
                NotificationUrl = "https://f81f-45-239-128-115.ngrok-free.app/api/Subscription/payment/callback" // **IMPORTANTE**: Use uma URL pública (ngrok para testes)
            };

            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);

            // Retorna a URL de checkout (init_point)
            return Result<string>.Success(preference.InitPoint);
        }
        catch (Exception ex)
        {
            // Logar o erro (ex.ToString()) seria uma boa prática aqui
            return Result<string>.Failure($"Erro ao criar preferência de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            var client = new MercadoPago.Client.Payment.PaymentClient();
            MercadoPago.Resource.Payment.Payment payment = await client.GetAsync(long.Parse(paymentId));

            return payment.Status switch
            {
                "approved" => Result<PaymentStatus>.Success(PaymentStatus.Approved),
                "rejected" => Result<PaymentStatus>.Success(PaymentStatus.Refused),
                _ => Result<PaymentStatus>.Success(PaymentStatus.Pending),
            };
        }
        catch (Exception ex)
        {
            return Result<PaymentStatus>.Failure($"Erro ao obter status do pagamento: {ex.Message}");
        }
    }
        
    public async Task<Result<string>> GetPaymentExternalReferenceAsync(string paymentId)
    {
        try
        {
            var client = new PaymentClient();
            var payment = await client.GetAsync(long.Parse(paymentId));

            if (payment == null || string.IsNullOrEmpty(payment.ExternalReference))
            {
                return Result<string>.Failure("Referência externa não encontrada no pagamento.");
            }

            return Result<string>.Success(payment.ExternalReference);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erro ao obter referência do pagamento: {ex.Message}");
        }
    }
}
