using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// API Controller for managing notifications
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsApiController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsApiController(
        INotificationService notificationService,
        ILogger<NotificationsApiController> logger) : base(logger)
  {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Get user's notifications
    /// GET /api/notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Notification>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] int count = 20, [FromQuery] bool includeRead = false)
    {
     var userId = GetUserId();
 var notifications = await _notificationService.GetUserNotificationsAsync(userId, count, includeRead);
  return Ok(notifications);
    }

    /// <summary>
    /// Get unread notification count
    /// GET /api/notifications/unread-count
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { unreadCount = count });
    }

    /// <summary>
    /// Get system announcements
    /// GET /api/notifications/announcements
    /// </summary>
    [HttpGet("announcements")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<Notification>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnnouncements()
    {
        var announcements = await _notificationService.GetSystemAnnouncementsAsync();
      return Ok(announcements);
    }

    /// <summary>
    /// Mark notification as read
    /// POST /api/notifications/{notificationId}/read
    /// </summary>
    [HttpPost("{notificationId:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
      var userId = GetUserId();
        var success = await _notificationService.MarkAsReadAsync(notificationId, userId);
        
        if (!success)
   return NotFound(new { message = "Notification not found" });

     return Ok(new { message = "Notification marked as read" });
    }

    /// <summary>
  /// Mark all notifications as read
    /// POST /api/notifications/read-all
    /// </summary>
    [HttpPost("read-all")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
  {
        var userId = GetUserId();
     var count = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { message = $"{count} notifications marked as read", count });
    }

    /// <summary>
  /// Delete a notification
/// DELETE /api/notifications/{notificationId}
    /// </summary>
    [HttpDelete("{notificationId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        var userId = GetUserId();
    var success = await _notificationService.DeleteNotificationAsync(notificationId, userId);
        
     if (!success)
return NotFound(new { message = "Notification not found" });

    return Ok(new { message = "Notification deleted" });
    }

    /// <summary>
    /// Delete all read notifications
  /// DELETE /api/notifications/read
    /// </summary>
    [HttpDelete("read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllRead()
    {
        var userId = GetUserId();
  var count = await _notificationService.DeleteAllReadNotificationsAsync(userId);
        return Ok(new { message = $"{count} read notifications deleted", count });
    }
}
