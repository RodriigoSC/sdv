using Infra.Cache;
using Infra.RateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDV.Application.Resolver;
using SDV.Infra.Consul;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Resolver;
using SDV.Infra.Vault;
using System;

namespace SDV.Infra.IoC
{
    public static class Bootstrap
    {
        // O método agora é síncrono (void), o que reflete seu comportamento real e remove o aviso.
        public static void StartIoC(IServiceCollection services, IConfiguration configuration)
        {
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

            // ------------------------
            // Vault
            // Registra a "receita" de como criar o VaultService.
            // Ele só será criado quando alguém realmente precisar dele.
            // ------------------------
            services.AddSingleton<IVaultService>(sp =>
            {
                // Agora lê da configuração injetada, que funciona tanto em prod quanto em teste.
                var vaultAddress = configuration["CONN_STRING_VAULT"] ?? throw new ArgumentNullException("CONN_STRING_VAULT", "Variável de ambiente não encontrada.");
                var vaultUser = configuration["USER_VAULT"] ?? throw new ArgumentNullException("USER_VAULT", "Variável de ambiente não encontrada.");
                var vaultPass = configuration["PASS_VAULT"] ?? throw new ArgumentNullException("PASS_VAULT", "Variável de ambiente não encontrada.");

                return new VaultService(vaultAddress, vaultUser, vaultPass);
            });

            // ------------------------
            // Consul
            // ------------------------
            services.AddSingleton<IConsulService>(sp =>
            {
                // Quando o Consul for necessário, ele primeiro resolverá o Vault.
                var vault = sp.GetRequiredService<IVaultService>();
                var mountPoint = $"sdv/{environment}";
                
                // .GetAwaiter().GetResult() é usado aqui porque a injeção de dependência síncrona não pode aguardar métodos assíncronos.
                var consulUrl = vault.GetKeyAsync("keys", "Consul.Url", mountPoint).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(consulUrl))
                    throw new InvalidOperationException("Consul URL not found in Vault.");

                return new ConsulService(consulUrl);
            });
            
            // ------------------------
            // MongoDB
            // ------------------------
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

            // ------------------------
            // Outros Serviços (Cache, RateLimit, etc.)
            // ------------------------
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddSingleton<IRateLimiterService>(sp => new MemoryRateLimiterService(100)); // Valor padrão, pode ser lido do Consul se necessário.

            // ------------------------
            // Repositórios e Serviços de Aplicação
            // ------------------------
            services.AddDataRepository();
            services.AddApplications();
        }
    }
}

