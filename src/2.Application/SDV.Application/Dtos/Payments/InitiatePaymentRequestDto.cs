namespace SDV.Application.Dtos.Payments;

public class InitiatePaymentRequestDto
{
    public string ClientId { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
}
