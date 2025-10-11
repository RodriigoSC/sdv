using System;

namespace SDV.Infra.RateLimit;

public interface IRateLimiterService
{
    Task<bool> IsAllowedAsync(string key);
}
