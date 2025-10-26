using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class RFQBidRepository : EfRepository<RFQBid>, IRFQBidRepository
{
    public RFQBidRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<RFQBid>> GetByRFQIdAsync(Guid rfqId)
        => await _db.RFQBids.Where(b => b.RFQId == rfqId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<RFQBid>> GetByTailorIdAsync(Guid tailorId)
        => await _db.RFQBids.Where(b => b.TailorId == tailorId).AsNoTracking().ToListAsync();

    public Task<RFQBid?> GetWinningBidAsync(Guid rfqId)
        => _db.RFQBids.AsNoTracking().OrderBy(b => b.BidAmount).FirstOrDefaultAsync(b => b.RFQId == rfqId);

    public async Task<bool> AcceptBidAsync(Guid bidId)
    {
        var bid = await _db.RFQBids.FindAsync(bidId);
        if (bid is null) return false;
        bid.Status = "Accepted";
        _db.RFQBids.Update(bid);
        return true;
    }

    public async Task<bool> RejectBidAsync(Guid bidId)
    {
        var bid = await _db.RFQBids.FindAsync(bidId);
        if (bid is null) return false;
        bid.Status = "Rejected";
        _db.RFQBids.Update(bid);
        return true;
    }

    public async Task<int> GetBidCountAsync(Guid rfqId)
        => await _db.RFQBids.CountAsync(b => b.RFQId == rfqId);
}
