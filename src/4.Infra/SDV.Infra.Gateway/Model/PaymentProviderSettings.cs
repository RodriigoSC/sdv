using SDV.Domain.Enums.Payments;

namespace SDV.Infra.Gateway.Model;

public class PaymentProviderSettings
{
    public PaymentProvider ActiveProvider { get; set; }
}
