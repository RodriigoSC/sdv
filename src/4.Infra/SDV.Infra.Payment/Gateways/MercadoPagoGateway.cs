using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Payment.Model.MercadoPago;

namespace SDV.Infra.Payment;

public class MercadoPagoGateway : IPaymentGateway
{
    private readonly MercadoPagoSettings _settings;
    private readonly ILogger<MercadoPagoGateway> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public MercadoPagoGateway(MercadoPagoSettings settings, ILogger<MercadoPagoGateway> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MercadoPagoConfig.AccessToken = _settings.AccessToken;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Falha ao chamar a API do Mercado Pago. Tentativa {RetryAttempt}. Aguardando {Delay}s. Erro: {Exception}",
                        retryAttempt, timespan.TotalSeconds, exception.Message);
                });
    }

    // ✅ CORRIGIDO: Assinatura padronizada
    public async Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return Result<string>.Failure("Valor do pagamento inválido");
            }

            if (request.Amount > 999999.99m)
            {
                return Result<string>.Failure("Valor excede o limite permitido");
            }

            var mercadoPagoRequest = new PreferenceRequest
            {
                ExternalReference = request.ExternalReference,
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = request.Description,
                        Description = request.Description,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = request.Amount,
                    },
                },
                BackUrls = new MercadoPago.Client.Preference.PreferenceBackUrlsRequest
                {
                    Success = request.ReturnSuccessUrl,
                    Failure = request.ReturnFailureUrl,
                    Pending = request.ReturnPendingUrl,
                },
                AutoReturn = "approved",
                NotificationUrl = !string.IsNullOrEmpty(request.WebhookSecret) 
                    ? $"{request.WebhookUrl}?secret={request.WebhookSecret}"
                    : request.WebhookUrl
            };

            var client = new PreferenceClient();
            Preference preference = await _retryPolicy.ExecuteAsync(() => client.CreateAsync(mercadoPagoRequest));

            return Result<string>.Success(preference.InitPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro final ao criar preferência de pagamento após todas as tentativas.");
            return Result<string>.Failure($"Erro ao criar preferência de pagamento: {ex.Message}");
        }
    }

    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            var client = new PaymentClient();
            var payment = await _retryPolicy.ExecuteAsync(() => client.GetAsync(long.Parse(paymentId)));

            return payment.Status switch
            {
                "approved" => Result<PaymentStatus>.Success(PaymentStatus.Approved),
                "rejected" => Result<PaymentStatus>.Success(PaymentStatus.Refunded),
                _ => Result<PaymentStatus>.Success(PaymentStatus.Pending),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro final ao obter status do pagamento {PaymentId} após todas as tentativas.", paymentId);
            return Result<PaymentStatus>.Failure($"Erro ao obter status do pagamento: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetPaymentExternalReferenceAsync(string paymentId)
    {
        try
        {
            var client = new PaymentClient();
            var payment = await _retryPolicy.ExecuteAsync(() => client.GetAsync(long.Parse(paymentId)));

            if (payment == null || string.IsNullOrEmpty(payment.ExternalReference))
            {
                return Result<string>.Failure("Referência externa não encontrada no pagamento.");
            }

            return Result<string>.Success(payment.ExternalReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro final ao obter referência externa do pagamento {PaymentId} após todas as tentativas.", paymentId);
            return Result<string>.Failure($"Erro ao obter referência do pagamento: {ex.Message}");
        }
    }

    public Result<bool> ValidateWebhookSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret) || secret != _settings.WebhookSecret)
        {
            _logger.LogWarning("Falha na validação do Webhook Secret.");
            return Result<bool>.Failure("Secret inválido.");
        }

        return Result<bool>.Success(true);
    }
}
