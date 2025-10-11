using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentProvider PaymentProvider { get; private set; }    
    public string TransactionId { get; private set; }
    public string PaymentUrl { get; private set; }
    public string QrCode { get; private set; }

    public Payment(Guid orderId, decimal amount, PaymentProvider paymentProvider)
    {
        OrderId = orderId;
        Amount = amount;
        PaymentProvider = paymentProvider;        
        Id = Guid.NewGuid();
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsApproved(string transactionId)
    {
        Status = PaymentStatus.Approved;
        TransactionId = transactionId;
        MarkAsUpdated();
    }

    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
        MarkAsUpdated();
    }
    
    public void AddGatewayResponseDetails(string paymentUrl, string qrCode)
    {
        PaymentUrl = paymentUrl;
        QrCode = qrCode;
        MarkAsUpdated();
    }
}
