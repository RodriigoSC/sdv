using SDV.Domain.Entities.Commons;
using SDV.Domain.Entities.Payments;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Interfaces.Payments;

/// <summary>
/// Interface para operações de domínio relacionadas a Pagamentos
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Cria uma URL de pagamento com o gateway externo
    /// </summary>
    Task<Result<string>> GeneratePaymentCheckoutAsync(PaymentGatewayRequest request);

    /// <summary>
    /// Obtém um pagamento pelo ID
    /// </summary>
    Task<Result<Payment>> GetPaymentByIdAsync(Guid paymentId);

    /// <summary>
    /// Obtém todos os pagamentos de um cliente
    /// </summary>
    Task<Result<IEnumerable<Payment>>> GetPaymentsByClientAsync(Guid clientId);

    /// <summary>
    /// Valida um callback de webhook do provedor
    /// </summary>
    Task<Result<bool>> ValidatePaymentCallbackAsync(string paymentId, string secret);

    /// <summary>
    /// Processa o callback de um pagamento aprovado
    /// </summary>
    Task<Result<Payment>> ProcessPaymentApprovalAsync(string paymentId);

    /// <summary>
    /// Processa o callback de um pagamento falhado
    /// </summary>
    Task<Result<bool>> ProcessPaymentFailureAsync(string paymentId, string reason);

    /// <summary>
    /// Obtém a instância do gateway apropriado
    /// </summary>
    IPaymentGateway GetPaymentGateway(PaymentProvider provider);
}
