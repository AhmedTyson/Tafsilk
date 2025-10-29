namespace TafsilkPlatform.Web.Interfaces
{
    public interface INotificationService
    {
        Task SendOrderStatusUpdateAsync(Guid orderId, string status);
        Task SendQuoteNotificationAsync(Guid tailorId, Guid orderId);
        Task SendReviewReminderAsync(Guid orderId);
        Task SendBulkNotificationAsync(IEnumerable<Guid> userIds, string title, string message);
    }
}
