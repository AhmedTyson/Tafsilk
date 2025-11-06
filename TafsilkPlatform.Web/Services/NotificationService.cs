using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for managing notifications in the Tafsilk platform
/// Supports personal notifications, system announcements, and real-time delivery
/// </summary>
public interface INotificationService
{
    // Send notifications
    Task<Guid> SendNotificationAsync(Guid userId, string title, string message, string type = "Info");
    Task<List<Guid>> SendBulkNotificationAsync(List<Guid> userIds, string title, string message, string type = "Info");
    Task<Guid> SendSystemAnnouncementAsync(string title, string message, string audienceType = "All", DateTime? expiresAt = null);
    
    // Get notifications
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int count = 20, bool includeRead = false);
    Task<List<Notification>> GetSystemAnnouncementsAsync(string? audienceType = null);
    Task<int> GetUnreadCountAsync(Guid userId);
    
    // Mark as read
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
  
 // Delete notifications
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
    Task<int> DeleteAllReadNotificationsAsync(Guid userId);
    
    // Notification templates
    Task SendOrderCreatedNotificationAsync(Guid tailorId, string orderNumber, decimal amount);
    Task SendOrderStatusChangedNotificationAsync(Guid customerId, string orderNumber, string newStatus);
  Task SendReviewReceivedNotificationAsync(Guid tailorId, string customerName, int rating);
    Task SendPaymentReceivedNotificationAsync(Guid tailorId, string orderNumber, decimal amount);
    Task SendVerificationApprovedNotificationAsync(Guid tailorId);
    Task SendVerificationRejectedNotificationAsync(Guid tailorId, string reason);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
     AppDbContext db,
        ILogger<NotificationService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
   _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ==================== SEND NOTIFICATIONS ====================

    /// <summary>
    /// Send a notification to a single user
    /// </summary>
    public async Task<Guid> SendNotificationAsync(Guid userId, string title, string message, string type = "Info")
    {
     try
        {
     var notification = new Notification
       {
      NotificationId = Guid.NewGuid(),
     UserId = userId,
         Title = title,
                Message = message,
          Type = type,
        IsRead = false,
       SentAt = DateTime.UtcNow,
 IsDeleted = false
            };

   await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();

  _logger.LogInformation("[NotificationService] Sent notification {NotificationId} to user {UserId}: {Title}",
     notification.NotificationId, userId, title);

      return notification.NotificationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationService] Error sending notification to user {UserId}", userId);
     throw;
        }
    }

    /// <summary>
    /// Send the same notification to multiple users
    /// </summary>
    public async Task<List<Guid>> SendBulkNotificationAsync(List<Guid> userIds, string title, string message, string type = "Info")
    {
   try
    {
      var notifications = userIds.Select(userId => new Notification
       {
  NotificationId = Guid.NewGuid(),
          UserId = userId,
  Title = title,
        Message = message,
 Type = type,
         IsRead = false,
  SentAt = DateTime.UtcNow,
       IsDeleted = false
   }).ToList();

            await _db.Notifications.AddRangeAsync(notifications);
            await _db.SaveChangesAsync();

        _logger.LogInformation("[NotificationService] Sent bulk notification to {Count} users: {Title}",
           userIds.Count, title);

    return notifications.Select(n => n.NotificationId).ToList();
        }
  catch (Exception ex)
        {
   _logger.LogError(ex, "[NotificationService] Error sending bulk notification to {Count} users", userIds.Count);
            throw;
        }
    }

    /// <summary>
    /// Send a system-wide announcement
    /// </summary>
    public async Task<Guid> SendSystemAnnouncementAsync(string title, string message, string audienceType = "All", DateTime? expiresAt = null)
    {
        try
        {
   var notification = new Notification
            {
         NotificationId = Guid.NewGuid(),
  UserId = null, // System message (no specific user)
                Title = title,
 Message = message,
       Type = "Announcement",
        AudienceType = audienceType,
    IsRead = false,
      SentAt = DateTime.UtcNow,
      ExpiresAt = expiresAt,
IsDeleted = false
          };

   await _db.Notifications.AddAsync(notification);
         await _db.SaveChangesAsync();

       _logger.LogInformation("[NotificationService] Sent system announcement {NotificationId}: {Title} to audience: {AudienceType}",
  notification.NotificationId, title, audienceType);

            return notification.NotificationId;
        }
        catch (Exception ex)
{
    _logger.LogError(ex, "[NotificationService] Error sending system announcement");
            throw;
        }
    }

    // ==================== GET NOTIFICATIONS ====================

    /// <summary>
    /// Get notifications for a specific user
    /// </summary>
    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int count = 20, bool includeRead = false)
    {
        try
      {
            var query = _db.Notifications
              .Where(n => n.UserId == userId && !n.IsDeleted);

   if (!includeRead)
 {
      query = query.Where(n => !n.IsRead);
        }

var notifications = await query
       .OrderByDescending(n => n.SentAt)
         .Take(count)
   .ToListAsync();

     return notifications;
        }
  catch (Exception ex)
     {
      _logger.LogError(ex, "[NotificationService] Error getting notifications for user {UserId}", userId);
     return new List<Notification>();
        }
    }

    /// <summary>
    /// Get active system announcements
    /// </summary>
    public async Task<List<Notification>> GetSystemAnnouncementsAsync(string? audienceType = null)
    {
     try
        {
      var query = _db.Notifications
         .Where(n => n.UserId == null && !n.IsDeleted) // System messages only
              .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow); // Not expired

            if (!string.IsNullOrEmpty(audienceType))
  {
            query = query.Where(n => n.AudienceType == audienceType || n.AudienceType == "All");
      }

     var announcements = await query
       .OrderByDescending(n => n.SentAt)
     .Take(10)
          .ToListAsync();

    return announcements;
        }
        catch (Exception ex)
{
         _logger.LogError(ex, "[NotificationService] Error getting system announcements");
            return new List<Notification>();
}
}

    /// <summary>
    /// Get count of unread notifications for a user
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
 {
            return await _db.Notifications
    .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
      }
        catch (Exception ex)
{
      _logger.LogError(ex, "[NotificationService] Error getting unread count for user {UserId}", userId);
       return 0;
 }
    }

    // ==================== MARK AS READ ====================

    /// <summary>
  /// Mark a notification as read
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        try
    {
   var notification = await _db.Notifications
        .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
      {
    _logger.LogWarning("[NotificationService] Notification {NotificationId} not found for user {UserId}",
     notificationId, userId);
  return false;
       }

  notification.IsRead = true;
       await _db.SaveChangesAsync();

 _logger.LogDebug("[NotificationService] Marked notification {NotificationId} as read", notificationId);

     return true;
        }
        catch (Exception ex)
        {
  _logger.LogError(ex, "[NotificationService] Error marking notification {NotificationId} as read", notificationId);
    return false;
        }
    }

    /// <summary>
  /// Mark all notifications as read for a user
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        try
      {
   var notifications = await _db.Notifications
        .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
        .ToListAsync();

         foreach (var notification in notifications)
   {
    notification.IsRead = true;
  }

        await _db.SaveChangesAsync();

         _logger.LogInformation("[NotificationService] Marked {Count} notifications as read for user {UserId}",
          notifications.Count, userId);

 return notifications.Count;
    }
   catch (Exception ex)
        {
        _logger.LogError(ex, "[NotificationService] Error marking all notifications as read for user {UserId}", userId);
   return 0;
        }
    }

    // ==================== DELETE NOTIFICATIONS ====================

    /// <summary>
    /// Delete (soft delete) a notification
  /// </summary>
    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
     try
    {
            var notification = await _db.Notifications
          .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
     {
          return false;
        }

            notification.IsDeleted = true;
     await _db.SaveChangesAsync();

            _logger.LogInformation("[NotificationService] Deleted notification {NotificationId}", notificationId);

    return true;
      }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotificationService] Error deleting notification {NotificationId}", notificationId);
      return false;
        }
    }

    /// <summary>
    /// Delete all read notifications for a user
    /// </summary>
    public async Task<int> DeleteAllReadNotificationsAsync(Guid userId)
    {
        try
        {
   var notifications = await _db.Notifications
          .Where(n => n.UserId == userId && n.IsRead && !n.IsDeleted)
       .ToListAsync();

            foreach (var notification in notifications)
            {
        notification.IsDeleted = true;
       }

     await _db.SaveChangesAsync();

         _logger.LogInformation("[NotificationService] Deleted {Count} read notifications for user {UserId}",
   notifications.Count, userId);

          return notifications.Count;
        }
        catch (Exception ex)
   {
            _logger.LogError(ex, "[NotificationService] Error deleting read notifications for user {UserId}", userId);
     return 0;
        }
    }

    // ==================== NOTIFICATION TEMPLATES ====================

    /// <summary>
    /// Send notification when a new order is created (to tailor)
    /// </summary>
    public async Task SendOrderCreatedNotificationAsync(Guid tailorId, string orderNumber, decimal amount)
    {
    await SendNotificationAsync(
     tailorId,
    "طلب جديد 🎉",
        $"لديك طلب جديد #{orderNumber} بقيمة {amount:C}. يرجى المراجعة والرد في أقرب وقت.",
         "Order"
 );
    }

    /// <summary>
  /// Send notification when order status changes (to customer)
    /// </summary>
    public async Task SendOrderStatusChangedNotificationAsync(Guid customerId, string orderNumber, string newStatus)
    {
        var statusArabic = newStatus switch
        {
  "Processing" => "قيد المعالجة",
            "InProgress" => "قيد التنفيذ",
            "Completed" => "مكتمل",
            "Delivered" => "تم التسليم",
     "Cancelled" => "ملغي",
    _ => newStatus
        };

        await SendNotificationAsync(
            customerId,
            "تحديث حالة الطلب",
      $"تم تحديث حالة طلبك #{orderNumber} إلى: {statusArabic}",
            "Order"
        );
    }

    /// <summary>
 /// Send notification when tailor receives a review
    /// </summary>
    public async Task SendReviewReceivedNotificationAsync(Guid tailorId, string customerName, int rating)
{
        var stars = string.Concat(Enumerable.Repeat("⭐", rating));
        await SendNotificationAsync(
            tailorId,
    "تقييم جديد",
            $"تلقيت تقييماً جديداً من {customerName}: {stars} ({rating}/5)",
            "Review"
 );
    }

    /// <summary>
    /// Send notification when payment is received
    /// </summary>
    public async Task SendPaymentReceivedNotificationAsync(Guid tailorId, string orderNumber, decimal amount)
    {
        await SendNotificationAsync(
            tailorId,
      "دفعة جديدة 💰",
        $"تم استلام دفعة بقيمة {amount:C} للطلب #{orderNumber}",
            "Payment"
        );
    }

    /// <summary>
    /// Send notification when tailor verification is approved
    /// </summary>
    public async Task SendVerificationApprovedNotificationAsync(Guid tailorId)
    {
        await SendNotificationAsync(
   tailorId,
       "تم التحقق من حسابك ✅",
        "مبروك! تم الموافقة على طلب التحقق الخاص بك. يمكنك الآن استقبال الطلبات.",
            "Verification"
        );
    }

    /// <summary>
    /// Send notification when tailor verification is rejected
    /// </summary>
    public async Task SendVerificationRejectedNotificationAsync(Guid tailorId, string reason)
    {
        await SendNotificationAsync(
  tailorId,
      "طلب التحقق مرفوض ❌",
        $"عذراً، تم رفض طلب التحقق الخاص بك. السبب: {reason}. يرجى تحديث المعلومات وإعادة المحاولة.",
    "Verification"
        );
    }
}
