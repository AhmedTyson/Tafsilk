using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Core.Models;
using TafsilkPlatform.Infrastructure.Persistence;

namespace TafsilkPlatform.Infrastructure.Repositories;

public class DisputeRepository : EfRepository<Dispute>, IDisputeRepository
{
    public DisputeRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Dispute>> GetByOrderIdAsync(Guid orderId)
        => await _db.Disputes.Where(d => d.OrderId == orderId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Dispute>> GetByUserIdAsync(Guid userId)
        => await _db.Disputes.Where(d => d.OpenedByUserId == userId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Dispute>> GetOpenDisputesAsync()
        => await _db.Disputes.Where(d => d.Status == "Open").AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Dispute>> GetDisputesByStatusAsync(string status)
        => await _db.Disputes.Where(d => d.Status == status).AsNoTracking().ToListAsync();

    public Task<Dispute?> GetDisputeWithOrderAsync(Guid disputeId)
        => _db.Disputes.Include(d => d.Order).AsNoTracking().FirstOrDefaultAsync(d => d.Id == disputeId);

    public async Task<bool> ResolveDisputeAsync(Guid disputeId, Guid adminId, string resolutionDetails)
    {
        var entity = await _db.Disputes.FindAsync(disputeId);
        if (entity is null) return false;
        entity.Status = "Resolved";
        entity.ResolvedByAdminId = adminId;
        entity.ResolutionDetails = resolutionDetails;
        entity.ResolvedAt = DateTime.UtcNow;
        _db.Disputes.Update(entity);
        return true;
    }

    public async Task<int> GetDisputeCountByStatusAsync(string status)
        => await _db.Disputes.CountAsync(d => d.Status == status);
}
