namespace SDV.Infra.Payment.Model.MercadoPago;

public class MercadoPagoSettings
{
    public string AccessToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string NotificationUrl { get; set; } = string.Empty;
    public BackUrls BackUrls { get; set; } = new();

    public MercadoPagoSettings() { }

    public MercadoPagoSettings(string accessToken, string webhookSecret, string notificationUrl, BackUrls backUrls)
    {
        AccessToken = accessToken;
        WebhookSecret = webhookSecret;
        NotificationUrl = notificationUrl;
        BackUrls = backUrls;
    }
}
