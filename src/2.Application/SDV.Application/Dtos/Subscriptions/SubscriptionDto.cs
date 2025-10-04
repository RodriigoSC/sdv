using System;
using SDV.Domain.Enums.Subscriptions;

namespace SDV.Application.Dtos.Subscriptions;

public class SubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public string TransactionId { get; set; } = string.Empty;

}
