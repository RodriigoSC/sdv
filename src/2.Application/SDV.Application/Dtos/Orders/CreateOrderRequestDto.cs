namespace SDV.Application.Dtos.Orders;

public class CreateOrderRequestDto
{
    public string ClientId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
}
