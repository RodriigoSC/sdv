using Infra.RateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDV.Application.Resolver;
using SDV.Domain.Interfaces.Payments;
using SDV.Infra.Cache;
using SDV.Infra.Consul;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Resolver;
using SDV.Infra.Payment;
using SDV.Infra.Payment.Model;
using SDV.Infra.Vault;
using System;

namespace SDV.Infra.IoC
{
    public static class Bootstrap
    {
        public static void StartIoC(IServiceCollection services, IConfiguration configuration)
        {
            // Configurações de logging e cache
            services.AddMemoryCache();
            services.AddLogging();

            // Adiciona os serviços de infraestrutura 
            services.AddVault(configuration);
            services.AddConsul(configuration);
            services.AddMongo(configuration);
            services.AddPaymentGateway(configuration);
            services.AddInfrastructureServices();

            // Registra as camadas de Repositório e Aplicação
            services.AddDataRepository();
            services.AddApplications();
        }

        private static IServiceCollection AddVault(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IVaultService>(sp =>
            {
                var vaultAddress = configuration["CONN_STRING_VAULT"] ?? throw new InvalidOperationException("Variável de ambiente 'CONN_STRING_VAULT' não encontrada.");
                var vaultUser = configuration["USER_VAULT"] ?? throw new InvalidOperationException("Variável de ambiente 'USER_VAULT' não encontrada.");
                var vaultPass = configuration["PASS_VAULT"] ?? throw new InvalidOperationException("Variável de ambiente 'PASS_VAULT' não encontrada.");
                return new VaultService(vaultAddress, vaultUser, vaultPass);
            });
        }

        private static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IConsulService>(sp =>
            {
                var vault = sp.GetRequiredService<IVaultService>();
                var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
                var mountPoint = $"sdv/{environment}";
                
                var consulUrl = vault.GetKeyAsync("keys", "Consul.Url", mountPoint).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(consulUrl))
                    throw new InvalidOperationException("Consul URL not found in Vault.");

                return new ConsulService(consulUrl);
            });
        }

        private static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton(sp =>
            {
                var vault = sp.GetRequiredService<IVaultService>();
                var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
                var mountPoint = $"sdv/{environment}";

                var mongoHost = vault.GetKeyAsync("keys", "MongoDb.host", mountPoint).GetAwaiter().GetResult();
                var mongoUser = vault.GetKeyAsync("keys", "MongoDb.user", mountPoint).GetAwaiter().GetResult();
                var mongoPass = vault.GetKeyAsync("keys", "MongoDb.pass", mountPoint).GetAwaiter().GetResult();
                var mongoDatabase = vault.GetKeyAsync("keys", "MongoDb.database", mountPoint).GetAwaiter().GetResult();
                
                if (string.IsNullOrEmpty(mongoDatabase))
                    throw new InvalidOperationException("Mongo database name not found in Vault.");

                var mongoConn = $"mongodb://{mongoUser}:{mongoPass}@{mongoHost}/{mongoDatabase}?authSource=admin";
                
                MongoDbConfig.Configure();
                return new MongoDBRepository(mongoConn).SetDatabase(mongoDatabase);
            });
        }

        private static IServiceCollection AddPaymentGateway(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Registra a classe de configurações
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

            return services.AddSingleton<IPaymentGateway>(sp =>
            {
                var settings = sp.GetRequiredService<MercadoPagoSettings>();
                var logger = sp.GetRequiredService<ILogger<MercadoPagoGateway>>();
                return new MercadoPagoGateway(settings, logger);
            });
        }

        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IRateLimiterService>(sp => new MemoryRateLimiterService(100));
            return services;
        }
    }
}