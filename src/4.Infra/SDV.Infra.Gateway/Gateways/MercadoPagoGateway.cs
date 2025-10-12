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
using SDV.Infra.Gateway.Model.MercadoPago;

namespace SDV.Infra.Gateway.Gateways;

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
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Falha na API do Mercado Pago. Tentativa {RetryAttempt}. Erro: {Exception}",
                        retryAttempt, exception.Message);
                });
    }

    public async Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request)
    {
        try
        {
            var mercadoPagoRequest = new PreferenceRequest
            {
                ExternalReference = request.ExternalReference,
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = request.Description,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = request.Amount,
                    },
                },
                NotificationUrl = _settings.NotificationUrl
            };

            var client = new PreferenceClient();
            Preference preference = await _retryPolicy.ExecuteAsync(() => client.CreateAsync(mercadoPagoRequest));

            return Result<string>.Success(preference.InitPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro final ao criar preferência de pagamento no Mercado Pago.");
            return Result<string>.Failure($"Erro ao criar preferência de pagamento: {ex.Message}");
        }
    }

    // ✅ IMPLEMENTADO CONFORME A INTERFACE
    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(string paymentId)
    {
        try
        {
            var client = new PaymentClient();
            var payment = await _retryPolicy.ExecuteAsync(() => client.GetAsync(long.Parse(paymentId)));

            if (payment == null)
            {
                return Result<PaymentStatus>.Failure("Pagamento não encontrado no gateway.");
            }

            var status = ConvertMercadoPagoStatus(payment.Status);
            return Result<PaymentStatus>.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do pagamento {PaymentId}.", paymentId);
            return Result<PaymentStatus>.Failure($"Erro ao obter status do pagamento: {ex.Message}");
        }
    }

    // ✅ IMPLEMENTADO CONFORME A INTERFACE
    public async Task<Result<string>> GetPaymentExternalReferenceAsync(string paymentId)
    {
        try
        {
            var client = new PaymentClient();
            var payment = await _retryPolicy.ExecuteAsync(() => client.GetAsync(long.Parse(paymentId)));

            if (payment == null || string.IsNullOrEmpty(payment.ExternalReference))
            {
                return Result<string>.Failure("Referência externa não encontrada para o pagamento.");
            }

            return Result<string>.Success(payment.ExternalReference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter referência externa do pagamento {PaymentId}.", paymentId);
            return Result<string>.Failure($"Erro ao obter referência do pagamento: {ex.Message}");
        }
    }

    // ✅ IMPLEMENTADO CONFORME A INTERFACE
    public Result<bool> ValidateWebhookSecret(string secret)
    {
        // Esta lógica deve ser melhorada para validar a assinatura completa do webhook
        // Mas para validar um "secret" simples, está correto.
        if (string.IsNullOrEmpty(secret) || secret != _settings.WebhookSecret)
        {
            _logger.LogWarning("Falha na validação do Webhook Secret.");
            return Result<bool>.Failure("Secret inválido.");
        }

        return Result<bool>.Success(true);
    }

    // ✔️ FUNÇÃO HELPER: Centraliza a conversão de status e corrige a lógica
    private PaymentStatus ConvertMercadoPagoStatus(string? mpStatus)
    {
        return mpStatus switch
        {
            "approved" => PaymentStatus.Approved,
            "rejected" => PaymentStatus.Failed,     // Corrigido: "rejeitado" é Falha, não Reembolsado
            "cancelled" => PaymentStatus.Cancelled,
            "refunded" => PaymentStatus.Refunded,
            "in_process" => PaymentStatus.Pending,
            _ => PaymentStatus.Pending,
        };
    }
}