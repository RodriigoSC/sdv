using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDV.Domain.Enums.Payments;
using SDV.Infra.Gateway.Factories;
using SDV.Infra.Gateway.Gateways;
using SDV.Infra.Gateway.Model.MercadoPago;
using SDV.Infra.Gateway.Model.PagarMe;
using SDV.Infra.Vault;

namespace SDV.Infra.Gateway.Resolver;

public static class ResolverIoc
{
    public static IServiceCollection AddPaymentGateway(this IServiceCollection services, IConfiguration configuration)
    {
        // Mercado Pago
        services.AddSingleton(sp =>
        {
            var vault = sp.GetRequiredService<IVaultService>();
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var mountPoint = $"sdv/{environment}";

            return new MercadoPagoSettings
            {
                AccessToken = vault.GetKeyAsync("keys", "MercadoPago.AccessToken", mountPoint).GetAwaiter().GetResult() ?? throw new InvalidOperationException("MercadoPago.AccessToken não encontrado no Vault."),
                NotificationUrl = vault.GetKeyAsync("keys", "MercadoPago.NotificationUrl", mountPoint).GetAwaiter().GetResult() ?? throw new InvalidOperationException("MercadoPago.NotificationUrl não encontrado no Vault."),
                WebhookSecret = vault.GetKeyAsync("keys", "MercadoPago.WebhookSecret", mountPoint).GetAwaiter().GetResult() ?? throw new InvalidOperationException("MercadoPago.WebhookSecret não encontrado no Vault."),
                BackUrls = new BackUrls
                {
                    Success = vault.GetKeyAsync("keys", "MercadoPago.BackUrlSuccess", mountPoint).GetAwaiter().GetResult() ?? "",
                    Failure = vault.GetKeyAsync("keys", "MercadoPago.BackUrlFailure", mountPoint).GetAwaiter().GetResult() ?? "",
                    Pending = vault.GetKeyAsync("keys", "MercadoPago.BackUrlPending", mountPoint).GetAwaiter().GetResult() ?? ""
                }
            };
        });

        // Pagar.me
        services.AddSingleton(sp =>
        {
            var vault = sp.GetRequiredService<IVaultService>();
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var mountPoint = $"sdv/{environment}";

            return new PagarMeSettings
            {
                SecretKey = vault.GetKeyAsync("keys", "PagarMe.SecretKey", mountPoint).GetAwaiter().GetResult() ?? throw new InvalidOperationException("PagarMe.SecretKey não encontrado no Vault."),
                WebhookSecretKey = vault.GetKeyAsync("keys", "PagarMe.WebhookSecretKey", mountPoint).GetAwaiter().GetResult() ?? throw new InvalidOperationException("PagarMe.WebhookSecretKey não encontrado no Vault.")
            };
        });

        // Registra os gateways
        services.AddSingleton<MercadoPagoGateway>();
        services.AddSingleton<PagarMeGateway>();

        // Registra a fábrica
        services.AddSingleton<GatewayProviderFactory>();

        // Registra o IPaymentGateway dinamicamente
        services.AddSingleton(sp =>
        {
            var vault = sp.GetRequiredService<IVaultService>();
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var mountPoint = $"sdv/{environment}";
            var factory = sp.GetRequiredService<GatewayProviderFactory>();

            var providerName = vault.GetKeyAsync("keys", "Payment.Provider", mountPoint).GetAwaiter().GetResult();
            if (Enum.TryParse<PaymentProvider>(providerName, true, out var provider))
            {
                return factory.GetProvider(provider);
            }

            throw new InvalidOperationException("Provedor de pagamento configurado no Vault é inválido.");
        });
        
        return services;
    }
}
