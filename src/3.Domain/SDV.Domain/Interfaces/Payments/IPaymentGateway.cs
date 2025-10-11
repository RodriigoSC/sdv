using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Interfaces.Payments;

/// <summary>
/// Interface padronizada para todos os gateways de pagamento.
/// Todos os gateways devem implementar exatamente essa assinatura.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Cria um pagamento através do gateway.
    /// </summary>
    Task<Result<string>> CreatePaymentAsync(PaymentGatewayRequest request);

    /// <summary>
    /// Obtém o status de um pagamento já processado.
    /// </summary>
    Task<Result<PaymentStatus>> GetPaymentStatusAsync(string paymentId);

    /// <summary>
    /// Obtém a referência externa (ID do pedido/subscription) de um pagamento.
    /// </summary>
    Task<Result<string>> GetPaymentExternalReferenceAsync(string paymentId);

    /// <summary>
    /// Valida o webhook secret para verificar autenticidade de callbacks.
    /// </summary>
    Result<bool> ValidateWebhookSecret(string secret);
}
