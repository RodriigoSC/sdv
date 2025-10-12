using SDV.Application.Dtos.Payments;
using SDV.Application.Results;

namespace SDV.Application.Interfaces;

/// <summary>
/// Interface de aplicação para operações de Pagamentos
/// Responsável por coordenar a criação de checkouts e processamento de callbacks
/// </summary>
public interface IPaymentApplication
{
    /// <summary>
    /// Inicia um checkout de pagamento para um cliente e plano
    /// </summary>
    Task<OperationResult<PaymentCheckoutResponseDto>> InitiatePaymentCheckoutAsync(InitiatePaymentRequestDto request);

    /// <summary>
    /// Processa o callback de aprovação de pagamento do provedor
    /// </summary>
    Task<OperationResult<PaymentDto>> ProcessPaymentApprovalCallbackAsync(PaymentWebhookCallbackDto callback);

    /// <summary>
    /// Processa o callback de falha de pagamento do provedor
    /// </summary>
    Task<OperationResult<bool>> ProcessPaymentFailureCallbackAsync(PaymentWebhookCallbackDto callback);

    /// <summary>
    /// Obtém detalhes de um pagamento
    /// </summary>
    Task<OperationResult<PaymentDto>> GetPaymentAsync(string paymentId);

    /// <summary>
    /// Obtém histórico de pagamentos de um cliente
    /// </summary>
    Task<OperationResult<IEnumerable<PaymentDto>>> GetPaymentHistoryAsync(string clientId);
}
