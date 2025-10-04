using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums.Subscriptions;

namespace SDV.Domain.Entities.Subscriptions;

public class Subscription : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string TransactionId { get; private set; }
    public string? LastProcessedPaymentId { get; private set; }
    public DateTime? LastPaymentProcessedAt { get; private set; }

    public Subscription(Guid clientId, Guid planId)
    {
        ClientId = clientId;
        PlanId = planId;
        StartDate = DateTime.UtcNow;
        Status = SubscriptionStatus.Pending;
        TransactionId = string.Empty;

    }

    public void Activate() => Status = SubscriptionStatus.Active;
    public void Expire() => Status = SubscriptionStatus.Expired;
    public void Fail() => Status = SubscriptionStatus.Failed;
    public void SetEndDate(DateTime endDate) => EndDate = endDate;
    public void SetTransactionId(string transactionId) => TransactionId = transactionId;
    public void SetLastProcessedPayment(string paymentId)
    {
        LastProcessedPaymentId = paymentId;
        LastPaymentProcessedAt = DateTime.UtcNow;
    }
}
