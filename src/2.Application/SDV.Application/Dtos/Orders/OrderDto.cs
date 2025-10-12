namespace SDV.Application.Dtos.Orders;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
}
