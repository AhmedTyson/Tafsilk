using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Abstraction for storing and retrieving idempotency keys and responses
/// Ensures that duplicate requests with the same key return the same response
/// </summary>
public interface IIdempotencyStore
{
/// <summary>
  /// Try to get a previously stored response for the given idempotency key
    /// </summary>
    /// <param name="key">Idempotency key</param>
    /// <param name="result">Stored response if found</param>
    /// <returns>True if response was found and is valid</returns>
Task<(bool Found, object? Result, int? StatusCode)> TryGetResponseAsync(string key);

    /// <summary>
    /// Save a response for the given idempotency key
    /// </summary>
    /// <param name="key">Idempotency key</param>
    /// <param name="result">Response object to store</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="userId">User ID who initiated the request</param>
    /// <param name="endpoint">Endpoint that was called</param>
    /// <param name="method">HTTP method</param>
    Task<bool> TrySaveResponseAsync(
        string key,
 object result,
        int statusCode,
        Guid? userId = null,
      string? endpoint = null,
   string? method = null);

    /// <summary>
    /// Mark an idempotency key as in progress
/// Prevents concurrent processing of the same key
    /// </summary>
    Task<bool> TryMarkAsInProgressAsync(
   string key,
   Guid? userId = null,
        string? endpoint = null,
        string? method = null);

    /// <summary>
    /// Mark an idempotency key as failed
 /// </summary>
    Task MarkAsFailedAsync(string key, string errorMessage);

    /// <summary>
    /// Clean up expired idempotency keys
    /// </summary>
    Task<int> CleanupExpiredKeysAsync();

    /// <summary>
    /// Check if a key is currently being processed
    /// </summary>
    Task<bool> IsInProgressAsync(string key);
}

/// <summary>
/// EF Core implementation of idempotency store
/// Persists idempotency keys and responses in the database
/// </summary>
public class EfCoreIdempotencyStore : IIdempotencyStore
{
    private readonly AppDbContext _db;
    private readonly ILogger<EfCoreIdempotencyStore> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public EfCoreIdempotencyStore(
        AppDbContext db,
   ILogger<EfCoreIdempotencyStore> logger)
    {
 _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Try to get a previously stored response
    /// </summary>
    public async Task<(bool Found, object? Result, int? StatusCode)> TryGetResponseAsync(string key)
    {
    try
        {
            var idempotencyKey = await _db.IdempotencyKeys
    .AsNoTracking()
      .FirstOrDefaultAsync(k => k.Key == key);

            if (idempotencyKey == null)
  {
          _logger.LogDebug("[IdempotencyStore] Key not found: {Key}", key);
          return (false, null, null);
            }

            // Check if key has expired
 if (idempotencyKey.ExpiresAtUtc < DateTime.UtcNow)
       {
   _logger.LogInformation("[IdempotencyStore] Key expired: {Key}", key);
             return (false, null, null);
            }

            // Check status
          if (idempotencyKey.Status == IdempotencyStatus.InProgress)
       {
     _logger.LogWarning("[IdempotencyStore] Key is currently being processed: {Key}", key);
        // Return a special indicator that processing is in progress
  return (false, null, 409); // Conflict
            }

 if (idempotencyKey.Status == IdempotencyStatus.Failed)
  {
      _logger.LogWarning("[IdempotencyStore] Key processing failed previously: {Key}, Error: {Error}",
 key, idempotencyKey.ErrorMessage);
             return (false, null, null);
 }

            if (idempotencyKey.Status != IdempotencyStatus.Completed)
   {
     _logger.LogWarning("[IdempotencyStore] Key has unexpected status: {Key}, Status: {Status}",
       key, idempotencyKey.Status);
           return (false, null, null);
 }

            // Update last accessed time
   idempotencyKey.LastAccessedAtUtc = DateTime.UtcNow;
            _db.IdempotencyKeys.Update(idempotencyKey);
            await _db.SaveChangesAsync();

            // Deserialize response
     if (string.IsNullOrEmpty(idempotencyKey.ResponseJson))
            {
       _logger.LogWarning("[IdempotencyStore] Key found but response is empty: {Key}", key);
          return (false, null, null);
       }

       var result = JsonSerializer.Deserialize<object>(idempotencyKey.ResponseJson, _jsonOptions);

      _logger.LogInformation("[IdempotencyStore] Retrieved cached response for key: {Key}, StatusCode: {StatusCode}",
    key, idempotencyKey.StatusCode);

      return (true, result, idempotencyKey.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[IdempotencyStore] Error retrieving response for key: {Key}", key);
     return (false, null, null);
}
    }

    /// <summary>
  /// Save a response for an idempotency key
    /// </summary>
    public async Task<bool> TrySaveResponseAsync(
        string key,
    object result,
        int statusCode,
 Guid? userId = null,
        string? endpoint = null,
        string? method = null)
  {
        try
        {
         var idempotencyKey = await _db.IdempotencyKeys.FindAsync(key);

            if (idempotencyKey == null)
  {
       // Create new entry
                idempotencyKey = new IdempotencyKey
       {
     Key = key,
        Status = IdempotencyStatus.Completed,
      ResponseJson = JsonSerializer.Serialize(result, _jsonOptions),
              StatusCode = statusCode,
          ContentType = "application/json",
        CreatedAtUtc = DateTime.UtcNow,
     ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
        UserId = userId,
        Endpoint = endpoint,
        Method = method
                };

       await _db.IdempotencyKeys.AddAsync(idempotencyKey);
      }
            else
{
          // Update existing entry
           idempotencyKey.Status = IdempotencyStatus.Completed;
     idempotencyKey.ResponseJson = JsonSerializer.Serialize(result, _jsonOptions);
          idempotencyKey.StatusCode = statusCode;
    idempotencyKey.ContentType = "application/json";
    idempotencyKey.LastAccessedAtUtc = DateTime.UtcNow;

    _db.IdempotencyKeys.Update(idempotencyKey);
   }

         await _db.SaveChangesAsync();

 _logger.LogInformation("[IdempotencyStore] Saved response for key: {Key}, StatusCode: {StatusCode}",
        key, statusCode);

    return true;
 }
        catch (Exception ex)
  {
            _logger.LogError(ex, "[IdempotencyStore] Error saving response for key: {Key}", key);
            return false;
  }
    }

    /// <summary>
    /// Mark a key as in progress to prevent concurrent processing
  /// </summary>
    public async Task<bool> TryMarkAsInProgressAsync(
        string key,
   Guid? userId = null,
        string? endpoint = null,
        string? method = null)
    {
        try
        {
         var existingKey = await _db.IdempotencyKeys.FindAsync(key);

    if (existingKey != null)
            {
        // Key already exists - check status
          if (existingKey.Status == IdempotencyStatus.InProgress)
       {
       _logger.LogWarning("[IdempotencyStore] Key is already in progress: {Key}", key);
          return false; // Another request is processing
  }

         if (existingKey.Status == IdempotencyStatus.Completed)
    {
      _logger.LogInformation("[IdempotencyStore] Key already completed: {Key}", key);
      return false; // Already processed
             }
   }

            var idempotencyKey = new IdempotencyKey
            {
           Key = key,
     Status = IdempotencyStatus.InProgress,
           CreatedAtUtc = DateTime.UtcNow,
          ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
         UserId = userId,
            Endpoint = endpoint,
                Method = method
            };

            await _db.IdempotencyKeys.AddAsync(idempotencyKey);
            await _db.SaveChangesAsync();

   _logger.LogInformation("[IdempotencyStore] Marked key as in progress: {Key}", key);

            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
        {
      // Race condition - another request created the key first
      _logger.LogWarning("[IdempotencyStore] Concurrent request detected for key: {Key}", key);
     return false;
        }
        catch (Exception ex)
    {
            _logger.LogError(ex, "[IdempotencyStore] Error marking key as in progress: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Mark a key as failed
    /// </summary>
    public async Task MarkAsFailedAsync(string key, string errorMessage)
    {
        try
      {
            var idempotencyKey = await _db.IdempotencyKeys.FindAsync(key);

         if (idempotencyKey != null)
            {
 idempotencyKey.Status = IdempotencyStatus.Failed;
   idempotencyKey.ErrorMessage = errorMessage;
        idempotencyKey.LastAccessedAtUtc = DateTime.UtcNow;

       _db.IdempotencyKeys.Update(idempotencyKey);
        await _db.SaveChangesAsync();

      _logger.LogInformation("[IdempotencyStore] Marked key as failed: {Key}, Error: {Error}",
                key, errorMessage);
            }
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "[IdempotencyStore] Error marking key as failed: {Key}", key);
        }
    }

    /// <summary>
    /// Clean up expired idempotency keys
    /// </summary>
    public async Task<int> CleanupExpiredKeysAsync()
    {
    try
        {
        var expiredKeys = await _db.IdempotencyKeys
         .Where(k => k.ExpiresAtUtc < DateTime.UtcNow || k.Status == IdempotencyStatus.Expired)
.ToListAsync();

            if (expiredKeys.Any())
         {
_db.IdempotencyKeys.RemoveRange(expiredKeys);
         await _db.SaveChangesAsync();

         _logger.LogInformation("[IdempotencyStore] Cleaned up {Count} expired keys", expiredKeys.Count);
            }

   return expiredKeys.Count;
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "[IdempotencyStore] Error cleaning up expired keys");
          return 0;
        }
    }

    /// <summary>
    /// Check if a key is currently being processed
    /// </summary>
    public async Task<bool> IsInProgressAsync(string key)
    {
        var idempotencyKey = await _db.IdempotencyKeys
         .AsNoTracking()
         .FirstOrDefaultAsync(k => k.Key == key);

        return idempotencyKey?.Status == IdempotencyStatus.InProgress;
    }
}

/// <summary>
/// In-memory implementation for testing/fallback
/// Not persistent across app restarts
/// </summary>
public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<string, IdempotencyKey> _store = new();
    private readonly ILogger<InMemoryIdempotencyStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public InMemoryIdempotencyStore(ILogger<InMemoryIdempotencyStore> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(bool Found, object? Result, int? StatusCode)> TryGetResponseAsync(string key)
    {
        await _lock.WaitAsync();
        try
    {
            if (_store.TryGetValue(key, out var idempotencyKey))
         {
           if (idempotencyKey.ExpiresAtUtc < DateTime.UtcNow)
         {
    _store.Remove(key);
            return (false, null, null);
          }

        if (idempotencyKey.Status == IdempotencyStatus.Completed && idempotencyKey.ResponseJson != null)
                {
var result = JsonSerializer.Deserialize<object>(idempotencyKey.ResponseJson);
      return (true, result, idempotencyKey.StatusCode);
      }
       }

   return (false, null, null);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> TrySaveResponseAsync(
        string key,
        object result,
        int statusCode,
        Guid? userId = null,
        string? endpoint = null,
        string? method = null)
    {
        await _lock.WaitAsync();
     try
        {
            _store[key] = new IdempotencyKey
          {
       Key = key,
  Status = IdempotencyStatus.Completed,
     ResponseJson = JsonSerializer.Serialize(result),
           StatusCode = statusCode,
     CreatedAtUtc = DateTime.UtcNow,
              ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
       UserId = userId,
     Endpoint = endpoint,
           Method = method
  };

       _logger.LogInformation("[InMemoryIdempotencyStore] Saved response for key: {Key}", key);
return true;
 }
        finally
        {
            _lock.Release();
        }
 }

    public async Task<bool> TryMarkAsInProgressAsync(
        string key,
        Guid? userId = null,
        string? endpoint = null,
        string? method = null)
    {
      await _lock.WaitAsync();
        try
        {
  if (_store.ContainsKey(key))
        return false;

    _store[key] = new IdempotencyKey
         {
              Key = key,
        Status = IdempotencyStatus.InProgress,
        CreatedAtUtc = DateTime.UtcNow,
      ExpiresAtUtc = DateTime.UtcNow.AddHours(24),
     UserId = userId,
                Endpoint = endpoint,
         Method = method
            };

         return true;
      }
        finally
        {
            _lock.Release();
        }
    }

    public async Task MarkAsFailedAsync(string key, string errorMessage)
    {
        await _lock.WaitAsync();
        try
        {
         if (_store.TryGetValue(key, out var idempotencyKey))
            {
   idempotencyKey.Status = IdempotencyStatus.Failed;
           idempotencyKey.ErrorMessage = errorMessage;
}
     }
     finally
        {
      _lock.Release();
        }
  }

    public async Task<int> CleanupExpiredKeysAsync()
    {
        await _lock.WaitAsync();
 try
        {
            var expiredKeys = _store
  .Where(kvp => kvp.Value.ExpiresAtUtc < DateTime.UtcNow)
                .Select(kvp => kvp.Key)
             .ToList();

            foreach (var key in expiredKeys)
        {
    _store.Remove(key);
       }

         return expiredKeys.Count;
        }
      finally
        {
            _lock.Release();
  }
    }

    public Task<bool> IsInProgressAsync(string key)
    {
        return Task.FromResult(_store.TryGetValue(key, out var idempotencyKey) &&
       idempotencyKey.Status == IdempotencyStatus.InProgress);
    }
}
