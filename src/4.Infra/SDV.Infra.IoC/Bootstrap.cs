using Infra.RateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDV.Application.Resolver;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Cache;
using SDV.Infra.Consul;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Resolver;
using SDV.Infra.Payment;
using SDV.Infra.Payment.Model;
using SDV.Infra.Vault;

namespace SDV.Infra.IoC
{
    public static class Bootstrap
    {
        public static void StartIoC(IServiceCollection services, IConfiguration configuration)
        {
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            
            services.AddSingleton<IVaultService>(sp =>
            {
                var vaultAddress = configuration["CONN_STRING_VAULT"]
                    ?? throw new InvalidOperationException("Variável de ambiente 'CONN_STRING_VAULT' não encontrada.");
                var vaultUser = configuration["USER_VAULT"]
                    ?? throw new InvalidOperationException("Variável de ambiente 'USER_VAULT' não encontrada.");
                var vaultPass = configuration["PASS_VAULT"]
                    ?? throw new InvalidOperationException("Variável de ambiente 'PASS_VAULT' não encontrada.");

                return new VaultService(vaultAddress, vaultUser, vaultPass);
            });

            services.AddSingleton<IConsulService>(sp =>
            {
                var vault = sp.GetRequiredService<IVaultService>();
                var mountPoint = $"sdv/{environment}";
                
                var consulUrl = vault.GetKeyAsync("keys", "Consul.Url", mountPoint).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(consulUrl))
                    throw new InvalidOperationException("Consul URL not found in Vault.");

                return new ConsulService(consulUrl);
            });
            
            
            services.AddSingleton(sp =>
            {
                var vault = sp.GetRequiredService<IVaultService>();
                var mountPoint = $"sdv/{environment}";

                var mongoHost = vault.GetKeyAsync("keys", "MongoDb.host", mountPoint).GetAwaiter().GetResult();
                var mongoUser = vault.GetKeyAsync("keys", "MongoDb.user", mountPoint).GetAwaiter().GetResult();
                var mongoPass = vault.GetKeyAsync("keys", "MongoDb.pass", mountPoint).GetAwaiter().GetResult();
                var mongoDatabase = vault.GetKeyAsync("keys", "MongoDb.database", mountPoint).GetAwaiter().GetResult();
                
                if (string.IsNullOrEmpty(mongoDatabase))
                    throw new InvalidOperationException("Mongo database name not found in Vault.");

                var mongoConn = $"mongodb://{mongoUser}:{mongoPass}@{mongoHost}/{mongoDatabase}?authSource=admin";
                
                return new MongoDBRepository(mongoConn).SetDatabase(mongoDatabase);
            });

            services.AddSingleton<IPaymentGateway>(sp =>
            {
                var vault = sp.GetRequiredService<IVaultService>();
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production";
                var mountPoint = $"sdv/{environment}";

                var apiKey = vault.GetKeyAsync("keys", "PaymentGateway.ApiKey", mountPoint).GetAwaiter().GetResult();
                var webhookSecret = vault.GetKeyAsync("keys", "PaymentGateway.WebhookSecret", mountPoint).GetAwaiter().GetResult();
                var notificationUrl = vault.GetKeyAsync("keys", "PaymentGateway.NotificationUrl", mountPoint).GetAwaiter().GetResult();

                var successUrl = vault.GetKeyAsync("keys", "PaymentGateway.BackUrls.Success", mountPoint).GetAwaiter().GetResult();
                var failureUrl = vault.GetKeyAsync("keys", "PaymentGateway.BackUrls.Failure", mountPoint).GetAwaiter().GetResult();
                var pendingUrl = vault.GetKeyAsync("keys", "PaymentGateway.BackUrls.Pending", mountPoint).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("Payment Gateway API key not found in Vault.");

                if (string.IsNullOrEmpty(webhookSecret))
                    throw new InvalidOperationException("Payment Gateway Webhook Secret not found in Vault.");

                if (string.IsNullOrEmpty(notificationUrl))
                    throw new InvalidOperationException("Payment Gateway NotificationUrl not found in Vault.");

                var backUrls = new BackUrls
                {
                    Success = successUrl ?? string.Empty,
                    Failure = failureUrl ?? string.Empty,
                    Pending = pendingUrl ?? string.Empty
                };

                var settings = new MercadoPagoSettings(apiKey, webhookSecret, notificationUrl, backUrls);

                return new MercadoPagoGateway(settings);
            });
            
            services.AddMemoryCache();            
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IRateLimiterService>(sp => new MemoryRateLimiterService(100)); 

            services.AddDataRepository();
            services.AddApplications();
        }
    }
}

