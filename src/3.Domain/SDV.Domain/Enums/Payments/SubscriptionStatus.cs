namespace SDV.Domain.Enums.Payments;

/// <summary>
/// Representa o status de uma assinatura.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// A assinatura está pendente de ativação.
    /// </summary>
    Pending,

    /// <summary>
    /// A assinatura está ativa.
    /// </summary>
    Active,

    /// <summary>
    /// A assinatura expirou.
    /// </summary>
    Expired,

    /// <summary>
    /// Houve uma falha no processamento da assinatura.
    /// </summary>
    Failed
}