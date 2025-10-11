using SDV.Domain.Enums.Orders;

namespace SDV.Application.Dtos.Orders;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public OrderStatus Status { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;

}
