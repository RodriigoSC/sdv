using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Entities.Payments;

// <summary>
/// Representa um pagamento realizado para um pedido.
/// Responsável por rastrear o status de cada transação.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// ID do pedido associado a este pagamento
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Valor do pagamento em reais (R$)
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Status atual do pagamento
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Provedor de pagamento utilizado (MercadoPago, Stripe, etc)
    /// </summary>
    public PaymentProvider Provider { get; private set; }

    /// <summary>
    /// ID único da transação no provedor externo
    /// </summary>
    public string? TransactionId { get; private set; }

    /// <summary>
    /// URL de checkout para o cliente realizar o pagamento
    /// </summary>
    public string? CheckoutUrl { get; private set; }

    /// <summary>
    /// QR Code para pagamentos instantâneos (ex: Pix)
    /// </summary>
    public string? QrCode { get; private set; }

    /// <summary>
    /// Data e hora em que o pagamento foi aprovado
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// Motivo da falha do pagamento (se aplicável)
    /// </summary>
    public string? FailureReason { get; private set; }

    public Payment(Guid orderId, decimal amount, PaymentProvider provider)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId é obrigatório", nameof(orderId));
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

        OrderId = orderId;
        Amount = amount;
        Provider = provider;
        Status = PaymentStatus.Pending;
    }

    /// <summary>
    /// Define a URL de checkout para o cliente
    /// </summary>
    public void SetCheckoutUrl(string checkoutUrl)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new ArgumentException("URL de checkout é obrigatória", nameof(checkoutUrl));

        CheckoutUrl = checkoutUrl;
        MarkAsUpdated();
    }

    /// <summary>
    /// Define QR Code para pagamento instantâneo
    /// </summary>
    public void SetQrCode(string qrCode)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
            throw new ArgumentException("QR Code é obrigatório", nameof(qrCode));

        QrCode = qrCode;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marca o pagamento como aprovado após receber webhook do provedor
    /// </summary>
    public void Approve(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("ID da transação é obrigatório", nameof(transactionId));

        Status = PaymentStatus.Approved;
        TransactionId = transactionId;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marca o pagamento como falho com motivo da falha
    /// </summary>
    public void Fail(string reason = "Motivo desconhecido")
    {
        if (Status == PaymentStatus.Approved)
            throw new InvalidOperationException("Não é possível falhar um pagamento já aprovado");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cancela um pagamento (apenas se ainda não foi aprovado)
    /// </summary>
    public void Cancel()
    {
        if (Status == PaymentStatus.Approved)
            throw new InvalidOperationException("Não é possível cancelar um pagamento já aprovado");

        Status = PaymentStatus.Cancelled;
        MarkAsUpdated();
    }

    /// <summary>
    /// Processa um reembolso do pagamento
    /// </summary>
    public void Refund()
    {
        if (Status != PaymentStatus.Approved)
            throw new InvalidOperationException("Apenas pagamentos aprovados podem ser reembolsados");

        Status = PaymentStatus.Refunded;
        MarkAsUpdated();
    }
}
