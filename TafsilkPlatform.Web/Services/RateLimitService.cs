using Microsoft.Extensions.Caching.Memory;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Rate limiting service to prevent brute force attacks
/// </summary>
public interface IRateLimitService
{
    Task<bool> IsRateLimitedAsync(string key);
    Task IncrementAsync(string key);
    Task ResetAsync(string key);
}

public class RateLimitService : IRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitService> _logger;
    
    // Configuration
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan SlidingWindow = TimeSpan.FromMinutes(5);

    public RateLimitService(IMemoryCache cache, ILogger<RateLimitService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<bool> IsRateLimitedAsync(string key)
    {
        var cacheKey = $"RateLimit_{key}";
        
    if (_cache.TryGetValue(cacheKey, out LoginAttemptInfo? info))
        {
            if (info!.IsLockedOut && info.LockoutUntil > DateTime.UtcNow)
          {
          _logger.LogWarning("[RateLimit] Key {Key} is locked out until {Until}", key, info.LockoutUntil);
    return Task.FromResult(true);
  }
            
       // Reset if lockout expired
            if (info.LockoutUntil <= DateTime.UtcNow)
            {
         _cache.Remove(cacheKey);
          return Task.FromResult(false);
            }
        }
        
        return Task.FromResult(false);
    }

public Task IncrementAsync(string key)
    {
        var cacheKey = $"RateLimit_{key}";
  
  var info = _cache.GetOrCreate(cacheKey, entry =>
        {
     entry.SlidingExpiration = SlidingWindow;
 return new LoginAttemptInfo();
   })!;

        info.AttemptCount++;
        info.LastAttempt = DateTime.UtcNow;

        if (info.AttemptCount >= MaxAttempts)
 {
            info.IsLockedOut = true;
 info.LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
            
   _logger.LogWarning("[RateLimit] Key {Key} locked out after {Attempts} attempts", key, info.AttemptCount);
            
        // Set absolute expiration for lockout
            _cache.Set(cacheKey, info, new MemoryCacheEntryOptions
            {
       AbsoluteExpiration = info.LockoutUntil
       });
        }
        else
        {
      _cache.Set(cacheKey, info);
        }

        return Task.CompletedTask;
    }

    public Task ResetAsync(string key)
    {
     var cacheKey = $"RateLimit_{key}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("[RateLimit] Reset for key: {Key}", key);
        return Task.CompletedTask;
    }

    private class LoginAttemptInfo
    {
      public int AttemptCount { get; set; }
        public DateTime LastAttempt { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LockoutUntil { get; set; }
    }
}
