using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Unified caching service supporting both in-memory and distributed caching
/// Provides automatic serialization/deserialization and cache invalidation
/// </summary>
public interface ICacheService
{
    // Get/Set operations
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    // Remove operations
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);

    // Existence check
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// In-memory cache implementation (fast, single-instance)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private static readonly HashSet<string> _keys = new();
    private static readonly SemaphoreSlim _keyLock = new(1, 1);

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("[CacheService] Cache HIT for key: {Key}", key);
                return Task.FromResult(value);
            }

            _logger.LogDebug("[CacheService] Cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CacheService] Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.SlidingExpiration = TimeSpan.FromMinutes(30);
            }

            _cache.Set(key, value, options);

            // Track key for prefix-based removal
            await _keyLock.WaitAsync();
            try
            {
                _keys.Add(key);
            }
            finally
            {
                _keyLock.Release();
            }

            _logger.LogDebug("[CacheService] Cached key: {Key} with expiration: {Expiration}",
 key, expiration?.ToString() ?? "30 minutes (sliding)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CacheService] Error setting cache value for key: {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);

        if (cachedValue != null)
            return cachedValue;

        _logger.LogDebug("[CacheService] Cache MISS for key: {Key}, calling factory", key);
        var value = await factory();

        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);

            _keyLock.Wait();
            try
            {
                _keys.Remove(key);
            }
            finally
            {
                _keyLock.Release();
            }

            _logger.LogDebug("[CacheService] Removed key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CacheService] Error removing cache key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            await _keyLock.WaitAsync();
            List<string> keysToRemove;
            try
            {
                keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
            }
            finally
            {
                _keyLock.Release();
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }

            _logger.LogInformation("[CacheService] Removed {Count} keys with prefix: {Prefix}",
                keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CacheService] Error removing keys by prefix: {Prefix}", prefix);
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}

/// <summary>
/// Distributed cache implementation (multi-instance, Redis/SQL Server)
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogDebug("[DistributedCache] Cache MISS for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("[DistributedCache] Cache HIT for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DistributedCache] Error getting cache value for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.SlidingExpiration = TimeSpan.FromMinutes(30);
            }

            await _cache.SetStringAsync(key, json, options);

            _logger.LogDebug("[DistributedCache] Cached key: {Key} with expiration: {Expiration}",
                   key, expiration?.ToString() ?? "30 minutes (sliding)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DistributedCache] Error setting cache value for key: {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);

        if (cachedValue != null)
            return cachedValue;

        _logger.LogDebug("[DistributedCache] Cache MISS for key: {Key}, calling factory", key);
        var value = await factory();

        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("[DistributedCache] Removed key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DistributedCache] Error removing cache key: {Key}", key);
        }
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        // Distributed cache doesn't support prefix-based removal out of the box
        // Would require Redis SCAN command or tracking keys separately
        _logger.LogWarning("[DistributedCache] RemoveByPrefix not fully supported for distributed cache: {Prefix}", prefix);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return !string.IsNullOrEmpty(value);
    }
}

/// <summary>
/// Cache key builder for consistent key naming
/// </summary>
public static class CacheKeys
{
    // User cache keys
    public static string User(Guid userId) => $"user:{userId}";
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";

    // Tailor cache keys
    public static string Tailor(Guid tailorId) => $"tailor:{tailorId}";
    public static string TailorList(string city = "all", int page = 1) => $"tailor:list:{city}:page:{page}";
    public static string TailorReviews(Guid tailorId) => $"tailor:reviews:{tailorId}";

    // Order cache keys
    public static string Order(Guid orderId) => $"order:{orderId}";
    public static string UserOrders(Guid userId) => $"user:orders:{userId}";

    // Notification cache keys
    public static string UserNotifications(Guid userId) => $"notifications:{userId}";
    public static string SystemAnnouncements() => "announcements:system";

    // Statistics cache keys
    public static string DashboardStats() => "dashboard:stats";
    public static string TailorStats(Guid tailorId) => $"tailor:stats:{tailorId}";
}
