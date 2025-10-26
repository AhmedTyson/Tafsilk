using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<IEnumerable<Notification>> GetRecentNotificationsAsync(Guid userId, int count);
    }
}
