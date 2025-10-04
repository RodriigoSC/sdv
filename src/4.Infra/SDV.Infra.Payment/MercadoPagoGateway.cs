using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Plans;
using SDV.Domain.Entities.Subscriptions;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Payment.Model;

namespace SDV.Infra.Payment;

public class MercadoPagoGateway : IPaymentGateway
{
    private readonly MercadoPagoSettings _settings;

    public MercadoPagoGateway(MercadoPagoSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        MercadoPagoConfig.AccessToken = _settings.AccessToken;
    }

    public async Task<Result<string>> CreatePaymentAsync(Subscription subscription, Plan plan)
    {
        try
        {
            if (plan.Price <= 0)
            {
                return Result<string>.Failure("Valor do plano inválido");
            }

            if (plan.Price > 999999.99m)
            {
                return Result<string>.Failure("Valor excede o limite permitido");
            }
            
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
                    Success = _settings.BackUrls.Success,
                    Failure = _settings.BackUrls.Failure,
                    Pending = _settings.BackUrls.Pending,
                },
                AutoReturn = "approved",
                NotificationUrl = $"{_settings.NotificationUrl}?secret={_settings.WebhookSecret}"

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
            var client = new PaymentClient();
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
    
     // Política de retry com backoff exponencial
    /*_retryPolicy = Policy<TResult>
        .Handle<HttpRequestException>()
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                _logger.LogWarning(
                    "Tentativa {RetryAttempt} falhou. Aguardando {Delay}s",
                    retryAttempt, timespan.TotalSeconds);
            });*/
}
