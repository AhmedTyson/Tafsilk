using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class WalletRepository : EfRepository<Wallet>, IWalletRepository
{
    public WalletRepository(AppDbContext db) : base(db) { }

    public Task<Wallet?> GetByUserIdAsync(Guid userId)
        => _db.Wallet.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId);

    public async Task<decimal> GetBalanceAsync(Guid userId)
        => await _db.Wallet.Where(w => w.UserId == userId).Select(w => w.Balance).FirstOrDefaultAsync();

    public Task<bool> CreditAsync(Guid userId, decimal amount, string description)
        => Task.FromResult(true); // placeholder ledger

    public Task<bool> DebitAsync(Guid userId, decimal amount, string description)
        => Task.FromResult(true);

    public Task<bool> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description)
        => Task.FromResult(true);
}
