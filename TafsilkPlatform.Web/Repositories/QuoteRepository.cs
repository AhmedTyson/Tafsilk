using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class QuoteRepository : EfRepository<Quote>, IQuoteRepository
{
    public QuoteRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Quote>> GetByOrderIdAsync(Guid orderId)
        => await _db.Quotes.Where(q => q.OrderId == orderId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Quote>> GetByTailorIdAsync(Guid tailorId)
        => await _db.Quotes.Where(q => q.TailorId == tailorId).AsNoTracking().ToListAsync();

    public Task<Quote?> GetQuoteWithOrderAsync(Guid quoteId)
        => _db.Quotes.Include(q => q.order).AsNoTracking().FirstOrDefaultAsync(q => q.QuoteId == quoteId);

    public async Task<bool> AcceptQuoteAsync(Guid quoteId)
    {
        var entity = await _db.Quotes.FindAsync(quoteId);
        if (entity is null) return false;
        // implement status change when Quote has status field
        _db.Quotes.Update(entity);
        return true;
    }

    public async Task<bool> RejectQuoteAsync(Guid quoteId)
    {
        var entity = await _db.Quotes.FindAsync(quoteId);
        if (entity is null) return false;
        // implement status change when Quote has status field
        _db.Quotes.Update(entity);
        return true;
    }

    public Task<Quote?> GetPendingQuoteAsync(Guid orderId, Guid tailorId)
        => _db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.OrderId == orderId && q.TailorId == tailorId);
}
