using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Payment>> GetByTailorIdAsync(Guid tailorId);
        Task<Payment?> GetPaymentWithOrderAsync(Guid paymentId);
        Task<decimal> GetTotalPaidAsync(Guid orderId);
        Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
    }
}
