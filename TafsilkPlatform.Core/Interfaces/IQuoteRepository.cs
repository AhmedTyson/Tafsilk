using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        Task<IEnumerable<Quote>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Quote>> GetByTailorIdAsync(Guid tailorId);
        Task<Quote?> GetQuoteWithOrderAsync(Guid quoteId);
        Task<bool> AcceptQuoteAsync(Guid quoteId);
        Task<bool> RejectQuoteAsync(Guid quoteId);
        Task<Quote?> GetPendingQuoteAsync(Guid orderId, Guid tailorId);
    }
}
