using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(Guid userId);
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<bool> CreditAsync(Guid userId, decimal amount, string description);
        Task<bool> DebitAsync(Guid userId, decimal amount, string description);
        Task<bool> TransferAsync(Guid fromUserId, Guid toUserId, decimal amount, string description);
    }
}
