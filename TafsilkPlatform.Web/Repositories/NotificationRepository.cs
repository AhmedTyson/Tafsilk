using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class NotificationRepository : EfRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        => await _db.Notifications.Where(n => n.UserId == userId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        => await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).AsNoTracking().ToListAsync();

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var entity = await _db.Notifications.FindAsync(notificationId);
        if (entity is null) return false;
        entity.IsRead = true;
        _db.Notifications.Update(entity);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var items = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        if (items.Count == 0) return false;
        foreach (var n in items) n.IsRead = true;
        _db.Notifications.UpdateRange(items);
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(Guid userId, int count)
        => await _db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.SentAt).Take(count).AsNoTracking().ToListAsync();
}
