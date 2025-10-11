using Microsoft.Extensions.Logging;
using PagarmeApiSDK.Standard.Models;     
using PagarmeApiSDK.Standard;   
using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Payments;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Payment.Model.PagarMe;
using PagarmeApiSDK.Standard.Exceptions;
using SDV.Domain.Entities.Payments;

namespace SDV.Infra.Payment.Gateways;


public class PagarMeGateway : IPaymentGateway
{
    private readonly PagarmeApiSDKClient _pagarmeClient;
    private readonly ILogger<PagarMeGateway> _logger;
    private readonly PagarMeSettings _settings;

    public PagarMeGateway(PagarMeSettings settings, ILogger<PagarMeGateway> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _pagarmeClient = new PagarmeApiSDKClient.Builder()
            .BasicAuthCredentials(basicAuthUserName: _settings.SecretKey, basicAuthPassword: "")
            .Build();
    }

    public async Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request)
    {
        try
        {
            var orderRequest = new CreateOrderRequest
            {
                Customer = new CreateCustomerRequest
                {
                    Name = request.CustomerName,
                    Email = request.CustomerEmail
                },
                Items = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        Amount = (int)(request.Amount * 100),
                        Description = request.Description,
                        Quantity = 1
                    }
                },
                Payments = new List<CreatePaymentRequest>
                {
                    new CreatePaymentRequest
                    {
                        PaymentMethod = "checkout",
                        Checkout = new CreateCheckoutPaymentRequest
                        {
                            AcceptedPaymentMethods = new List<string> { "credit_card", "boleto", "pix" },
                            SuccessUrl = request.ReturnSuccessUrl,
                            SkipCheckoutSuccessPage = true
                        }
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "external_reference", request.ExternalReference }
                }
            };

            var response = await _pagarmeClient.OrdersController.CreateOrderAsync(orderRequest);

            var checkoutUrl = response?.Checkouts?.FirstOrDefault()?.PaymentUrl;

            if (!string.IsNullOrEmpty(checkoutUrl))
            {
                return Result<string>.Success(checkoutUrl);
            }

            _logger.LogWarning("A Pagar.me não retornou uma URL de checkout no objeto Order.");
            return Result<string>.Failure("Não foi possível obter a URL de checkout da Pagar.me.");
        }
        catch (ErrorException ex)
        {
            _logger.LogError(ex, "Erro da API Pagar.me ao criar pedido com checkout: {Response}", ex.Message);
            return Result<string>.Failure($"Erro na API da Pagar.me: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar pedido com checkout na Pagar.me.");
            return Result<string>.Failure($"Erro ao criar pagamento na Pagar.me: {ex.Message}");
        }
    }

    public async Task<Result<PaymentStatus>> GetPaymentStatusAsync(string chargeId)
    {
        try
        {
            var charge = await _pagarmeClient.ChargesController.GetChargeAsync(chargeId);
            return charge.Status switch
            {
                "paid" => Result<PaymentStatus>.Success(PaymentStatus.Approved),
                "failed" => Result<PaymentStatus>.Success(PaymentStatus.Refunded),
                _ => Result<PaymentStatus>.Success(PaymentStatus.Pending),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do pagamento na Pagar.me para o chargeId {ChargeId}", chargeId);
            return Result<PaymentStatus>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> GetPaymentExternalReferenceAsync(string chargeId)
    {
        try
        {
            var charge = await _pagarmeClient.ChargesController.GetChargeAsync(chargeId);
            if (charge?.Metadata != null && charge.Metadata.TryGetValue("external_reference", out var externalReference))
            {
                return Result<string>.Success(externalReference);
            }
            return Result<string>.Failure("Metadado 'external_reference' não encontrado no charge da Pagar.me.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter referência externa do pagamento na Pagar.me para o chargeId {ChargeId}", chargeId);
            return Result<string>.Failure(ex.Message);
        }
    }

    public Result<bool> ValidateWebhookSecret(string secret)
    {
        if (string.IsNullOrEmpty(secret) || secret != _settings.WebhookSecretKey)
        {
            _logger.LogWarning("Falha na validação do Webhook Secret da Pagar.me.");
            return Result<bool>.Failure("Secret inválido.");
        }
        return Result<bool>.Success(true);
    }
}