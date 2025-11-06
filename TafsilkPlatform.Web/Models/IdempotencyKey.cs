using System.ComponentModel.DataAnnotations;

namespace TafsilkPlatform.Web.Models;

/// <summary>
/// Represents an idempotency key for preventing duplicate requests
/// Ensures that POST requests with the same key are processed only once
/// </summary>
public class IdempotencyKey
{
    [Key]
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Status of the idempotency key processing
    /// </summary>
    public IdempotencyStatus Status { get; set; } = IdempotencyStatus.InProgress;

    /// <summary>
    /// JSON serialized response that was returned for this key
    /// </summary>
    public string? ResponseJson { get; set; }

    /// <summary>
    /// HTTP status code of the original response
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Content type of the response (e.g., application/json)
    /// </summary>
    [MaxLength(100)]
    public string? ContentType { get; set; }

    /// <summary>
    /// When this key was first created
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this key was last accessed
    /// </summary>
  public DateTime? LastAccessedAtUtc { get; set; }

    /// <summary>
    /// When this key expires and can be cleaned up
    /// Default: 24 hours from creation
    /// </summary>
public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddHours(24);

    /// <summary>
    /// User ID who initiated the request (for tracking)
  /// </summary>
  public Guid? UserId { get; set; }

    /// <summary>
    /// Endpoint that was called (e.g., /api/orders)
    /// </summary>
    [MaxLength(500)]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Request method (POST, PUT, etc.)
    /// </summary>
    [MaxLength(10)]
    public string? Method { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status of idempotency key processing
/// </summary>
public enum IdempotencyStatus
{
    /// <summary>
    /// Request is currently being processed
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Request completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Request failed
  /// </summary>
    Failed = 2,

/// <summary>
    /// Key has expired and can be cleaned up
    /// </summary>
    Expired = 3
}
