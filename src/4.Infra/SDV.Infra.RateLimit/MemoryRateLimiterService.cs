using System.Threading.RateLimiting;


namespace SDV.Infra.RateLimit;

public class MemoryRateLimiterService : IRateLimiterService
{
    private readonly RateLimiter _limiter;

    public MemoryRateLimiterService(int maxRequestsPerMinute = 10)
    {
        _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = maxRequestsPerMinute,
            TokensPerPeriod = maxRequestsPerMinute,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    public async Task<bool> IsAllowedAsync(string key)
    {
        var lease = await _limiter.AcquireAsync(1);
        return lease.IsAcquired;
    }
}
