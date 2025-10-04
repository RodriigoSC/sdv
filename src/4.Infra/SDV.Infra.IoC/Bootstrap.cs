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
            
            
            services.AddSingleton<IMongoDBRepository>(sp =>
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
                var mountPoint = $"sdv/{environment}";

                var apiKey = vault.GetKeyAsync("keys", "PaymentGateway.ApiKey", mountPoint).GetAwaiter().GetResult();
                
                if (string.IsNullOrEmpty(apiKey))
                    throw new InvalidOperationException("Payment Gateway API key not found in Vault.");

                return new MercadoPagoGateway(apiKey);
            });
            
            services.AddMemoryCache();            
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IRateLimiterService>(sp => new MemoryRateLimiterService(100)); 

            services.AddDataRepository();
            services.AddApplications();
        }
    }
}

