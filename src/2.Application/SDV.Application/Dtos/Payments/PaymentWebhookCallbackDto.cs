namespace SDV.Application.Dtos.Payments;

public class PaymentWebhookCallbackDto

{
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? Secret { get; set; }
}
