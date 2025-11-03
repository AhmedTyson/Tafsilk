using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Caching service interface
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

/// <summary>
/// Distributed cache service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
     _cache = cache;
_logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
 try
        {
   var cachedData = await _cache.GetStringAsync(key, cancellationToken);
      if (string.IsNullOrEmpty(cachedData))
    {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
   catch (Exception ex)
        {
    _logger.LogError(ex, "Error getting cached data for key: {Key}", key);
       return default;
        }
 }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
      var serializedData = JsonSerializer.Serialize(value, _jsonOptions);
   var options = new DistributedCacheEntryOptions
   {
          AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
     };

      await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
        }
 catch (Exception ex)
 {
   _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
      {
  await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Error removing cache for key: {Key}", key);
     }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Note: This requires a more advanced implementation with Redis or a cache that supports pattern deletion
  // For now, this is a placeholder
   _logger.LogWarning("RemoveByPrefixAsync not fully implemented for distributed cache. Prefix: {Prefix}", prefix);
  await Task.CompletedTask;
    }
}

/// <summary>
/// Cache key builder for consistent key generation
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "Tafsilk:";

    public static string TailorProfile(Guid tailorId) => $"{Prefix}Tailor:{tailorId}";
    public static string TailorsByCity(string city) => $"{Prefix}Tailors:City:{city}";
    public static string TopRatedTailors(string? city = null) => 
  city != null ? $"{Prefix}Tailors:TopRated:{city}" : $"{Prefix}Tailors:TopRated:All";
    public static string CustomerProfile(Guid customerId) => $"{Prefix}Customer:{customerId}";
    public static string User(Guid userId) => $"{Prefix}User:{userId}";
    public static string UserByEmail(string email) => $"{Prefix}User:Email:{email}";
    public static string TailorServices(Guid tailorId) => $"{Prefix}TailorServices:{tailorId}";
    public static string PortfolioImages(Guid tailorId) => $"{Prefix}Portfolio:{tailorId}";
}
