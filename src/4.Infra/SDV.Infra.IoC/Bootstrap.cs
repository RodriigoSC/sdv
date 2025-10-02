using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using SDV.Infra.Consul;
using SDV.Infra.Vault;
using SDV.Infra.Data.Resolver;
using SDV.Infra.Data.MongoDB;
using Infra.Cache;
using Infra.RateLimit;
using SDV.Application.Resolver;

namespace SDV.Infra.IoC;

public static class Bootstrap
{
    public static async Task StartIoCAsync(IServiceCollection services, IConfiguration configuration)
    {        
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? throw new ArgumentNullException("ASPNETCORE_ENVIRONMENT");

        // ------------------------
        // Vault
        // ------------------------
        services.AddSingleton<IVaultService>(sp =>
        {
            var vaultAddress = Environment.GetEnvironmentVariable("CONN_STRING_VAULT")
                ?? throw new ArgumentNullException("CONN_STRING_VAULT");
            var vaultUser = Environment.GetEnvironmentVariable("USER_VAULT")
                ?? throw new ArgumentNullException("USER_VAULT");
            var vaultPass = Environment.GetEnvironmentVariable("PASS_VAULT")
                ?? throw new ArgumentNullException("PASS_VAULT");

            return new VaultService(vaultAddress, vaultUser, vaultPass);
        });

        // Build temporário apenas para pegar VaultService
        var spTemp = services.BuildServiceProvider();
        var vault = spTemp.GetRequiredService<IVaultService>();

        var mountPoint = $"sdv/{environment}";

        // ------------------------
        // Consul (busca URL do Vault)
        // ------------------------
        var consulUrl = await vault.GetKeyAsync("keys", "Consul.Url", mountPoint);
        if (string.IsNullOrEmpty(consulUrl))
            throw new InvalidOperationException("Consul URL not found in Vault.");

        // Registra o ConsulService de verdade
        services.AddSingleton<IConsulService>(new ConsulService(consulUrl));

        // Build temporário para pegar ConsulService agora que ele foi registrado
        spTemp = services.BuildServiceProvider();
        var consul = spTemp.GetRequiredService<IConsulService>();

        // ------------------------
        // Resolução de valores do Consul
        // ------------------------
        var cacheExpirationStr = await consul.GetValueAsync("sdv/config/cache", "CacheExpirationMinutes");
        var cacheExpiration = int.TryParse(cacheExpirationStr, out var ce) ? ce : 5;

        var rateLimitStr = await consul.GetValueAsync("sdv/config/rate", "MaxRequestsPerMinute");
        var rateLimit = int.TryParse(rateLimitStr, out var rl) ? rl : 10;

        // ------------------------
        // Repositórios genéricos
        // ------------------------
        services.AddDataRepository();
        services.AddApplications();

        // ------------------------
        // MemoryCache
        // ------------------------
        services.AddMemoryCache();
        services.AddMemoryCache();

        services.AddSingleton<ICacheService>(sp =>
        {
            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            return new MemoryCacheService(memoryCache, TimeSpan.FromMinutes(cacheExpiration));
        });


        // ------------------------
        // MongoDB
        // ------------------------
        var mongoHost = await vault.GetKeyAsync("keys", "MongoDb.host", mountPoint);
        var mongoUser = await vault.GetKeyAsync("keys", "MongoDb.user", mountPoint);
        var mongoPass = await vault.GetKeyAsync("keys", "MongoDb.pass", mountPoint);
        var mongoDatabase = await vault.GetKeyAsync("keys", "MongoDb.database", mountPoint);
        if (string.IsNullOrEmpty(mongoDatabase))
            throw new InvalidOperationException("Mongo database name not found in Vault.");

        var mongoConn = $"mongodb://{mongoUser}:{mongoPass}@{mongoHost}/{mongoDatabase}?authSource=admin";

        // ------------------------
        // Registro final dos singletons
        // ------------------------
        services.AddSingleton<IRateLimiterService>(new MemoryRateLimiterService(rateLimit));
        services.AddSingleton<IMongoDBRepository>(new MongoDBRepository(mongoConn).SetDatabase(mongoDatabase));
    }
}
