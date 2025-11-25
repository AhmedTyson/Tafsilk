using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository;

public class PaymentRepository : EfRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId)
        => await _db.Payment.Where(p => p.OrderId == orderId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(Guid customerId)
        => await _db.Payment.Where(p => p.CustomerId == customerId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Payment>> GetByTailorIdAsync(Guid tailorId)
        => await _db.Payment.Where(p => p.TailorId == tailorId).AsNoTracking().ToListAsync();

    public Task<Payment?> GetPaymentWithOrderAsync(Guid paymentId)
        => _db.Payment.Include(p => p.Order).AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == paymentId);

    public async Task<decimal> GetTotalPaidAsync(Guid orderId)
        => await _db.Payment.Where(p => p.OrderId == orderId).SumAsync(p => p.Amount);

    public Task<bool> ProcessPaymentAsync(Guid paymentId, string transactionId)
        => Task.FromResult(true); // placeholder

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        => await _db.Payment.Where(p => p.PaymentStatus == Enums.PaymentStatus.Pending).AsNoTracking().ToListAsync();
}
