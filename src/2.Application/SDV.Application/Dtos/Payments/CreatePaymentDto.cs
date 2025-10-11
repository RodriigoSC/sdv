namespace SDV.Application.Dtos.Payments;

public class CreatePaymentDto
{
    public string SubscriptionId { get; set; }
    public string CheckoutUrl { get; set; }

    public CreatePaymentDto(string subscriptionId, string checkoutUrl)
    {
        SubscriptionId = subscriptionId;
        CheckoutUrl = checkoutUrl;
    }

}
