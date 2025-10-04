using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Payments;

namespace SDV.Domain.Entities.Payments;

public class Payment: BaseEntity
{
    public Guid SubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; }

    public Payment(Guid subscriptionId, decimal amount, string transactionId)
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Pending;
        TransactionId = transactionId;
    }

    public void Approve() => Status = PaymentStatus.Approved;
    public void Refuse() => Status = PaymentStatus.Refused;
}
