namespace SDV.Domain.Enums.Payments;

/// <summary>
/// Representa o status de um pagamento.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// O pagamento está pendente de processamento.
    /// </summary>
    Pending,

    /// <summary>
    /// O pagamento foi aprovado.
    /// </summary>
    Approved,

    /// <summary>
    /// O pagamento foi recusado.
    /// </summary>
    Refused
}