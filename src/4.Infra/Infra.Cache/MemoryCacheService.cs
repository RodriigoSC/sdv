using Microsoft.Extensions.Caching.Memory;

namespace SDV.Infra.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;

    public MemoryCacheService(IMemoryCache cache, TimeSpan? defaultExpiration = null)
    {
        _cache = cache;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(5);
    }

    public Task<T?> GetAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out object? cachedValue) && cachedValue is T value)
        {
            return Task.FromResult<T?>(value);
        }

        return Task.FromResult<T?>(default);
    } 
    
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _cache.Set(key, value, expiration ?? _defaultExpiration);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
