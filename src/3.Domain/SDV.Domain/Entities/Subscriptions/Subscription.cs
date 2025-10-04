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

    public Subscription(Guid clientId, Guid planId)
    {
        ClientId = clientId;
        PlanId = planId;
        StartDate = DateTime.UtcNow;
        Status = SubscriptionStatus.Pending;
    }

    public void Activate() => Status = SubscriptionStatus.Active;
    public void Expire() => Status = SubscriptionStatus.Expired;
    public void SetEndDate(DateTime endDate) => EndDate = endDate;
}
