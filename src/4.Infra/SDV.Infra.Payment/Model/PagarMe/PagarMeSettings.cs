namespace SDV.Infra.Payment.Model.PagarMe;

public class PagarMeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecretKey { get; set; } = string.Empty;
}
