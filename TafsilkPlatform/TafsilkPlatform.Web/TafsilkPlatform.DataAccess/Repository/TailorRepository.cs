using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository;

public class TailorRepository : EfRepository<TailorProfile>, ITailorRepository
{
    public TailorRepository(ApplicationDbContext db) : base(db) { }

    public Task<TailorProfile?> GetByUserIdAsync(Guid userId)
        => _db.TailorProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId);

    public async Task<IEnumerable<TailorProfile>> GetByLocationAsync(decimal latitude, decimal longitude, double radiusKm)
    {
        // Simplified location-based search
        // In production, you might use spatial queries or a more sophisticated algorithm
        return await _db.TailorProfiles
            .AsNoTracking()
            .Where(t => t.IsVerified)
            .ToListAsync();
    }

    public async Task<IEnumerable<TailorProfile>> GetByServiceTypeAsync(string serviceType)
    {
        return await _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.TailorServices)
            .Where(t => t.IsVerified && t.TailorServices.Any(s => s.ServiceName == serviceType))
            .ToListAsync();
    }

    public async Task<IEnumerable<TailorProfile>> GetAvailableTailorsAsync(DateTime date)
    {
        // Get all verified tailors (availability logic can be enhanced)
        return await _db.TailorProfiles
            .AsNoTracking()
            .Where(t => t.IsVerified)
            .ToListAsync();
    }

    public async Task<IEnumerable<TailorProfile>> GetVerifiedTailorsAsync()
    {
        return await _db.TailorProfiles
            .AsNoTracking()
            .Where(t => t.IsVerified)
            .ToListAsync();
    }

    public async Task<IEnumerable<TailorProfile>> SearchTailorsAsync(string searchTerm, string? city = null)
    {
        var query = _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.IsVerified);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t =>
                (t.Bio != null && t.Bio.Contains(searchTerm)) ||
                t.ShopName.Contains(searchTerm) ||
                (t.FullName != null && t.FullName.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(t => t.City == city);
        }

        return await query.ToListAsync();
    }

    public Task<TailorProfile?> GetTailorWithServicesAsync(Guid tailorId)
        => _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.TailorServices)
            .FirstOrDefaultAsync(t => t.Id == tailorId);

    public Task<TailorProfile?> GetTailorWithPortfolioAsync(Guid tailorId)
        => _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.PortfolioImages)
            .FirstOrDefaultAsync(t => t.Id == tailorId);

    public async Task<TailorProfile?> GetTailorWithReviewsAsync(Guid tailorId)
    {
        // Note: TailorProfile doesn't have a Reviews navigation property
        // Reviews are tracked separately in the system
        return await _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tailorId);
    }

    public async Task UpdateTailorRatingAsync(Guid tailorId)
    {
        // Note: Rating updates should be handled by the service layer
        // which has access to the review data
        var tailor = await _db.TailorProfiles
            .FirstOrDefaultAsync(t => t.Id == tailorId);

        if (tailor == null)
            return;

        // The rating calculation logic should be in the service layer
        // This method just ensures the tailor exists
        _db.TailorProfiles.Update(tailor);
    }

    public async Task<IEnumerable<TailorProfile>> GetTopRatedTailorsAsync(int count)
    {
        return await _db.TailorProfiles
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.IsVerified)
            .OrderByDescending(t => t.AverageRating)
            .Take(count)
            .ToListAsync();
    }
}
