using System;

namespace Infra.RateLimit;

public interface IRateLimiterService
{
    Task<bool> IsAllowedAsync(string key);
}
