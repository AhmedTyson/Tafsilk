using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class TailorRepository : EfRepository<TailorProfile>, ITailorRepository
{
    public TailorRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<TailorProfile>> GetByLocationAsync(decimal latitude, decimal longitude, double radiusKm)
    {
        // simple box filter; replace with proper geospatial if needed
        var latDelta = (decimal)(radiusKm / 111.0);
        var lonDelta = (decimal)(radiusKm / 111.0);
        return await _db.TailorProfiles
            .Where(t => t.Latitude != null && t.Longitude != null
                && t.Latitude >= latitude - latDelta && t.Latitude <= latitude + latDelta
                && t.Longitude >= longitude - lonDelta && t.Longitude <= longitude + lonDelta)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<IEnumerable<TailorProfile>> GetByServiceTypeAsync(string serviceType)
        => Task.FromResult<IEnumerable<TailorProfile>>(Array.Empty<TailorProfile>());

    public Task<IEnumerable<TailorProfile>> GetAvailableTailorsAsync(DateTime date)
        => Task.FromResult<IEnumerable<TailorProfile>>(Array.Empty<TailorProfile>());

    public async Task<IEnumerable<TailorProfile>> GetVerifiedTailorsAsync()
        => await _db.TailorProfiles.Where(t => t.IsVerified).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<TailorProfile>> SearchTailorsAsync(string searchTerm, string? city = null)
    {
        var query = _db.TailorProfiles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(t => t.ShopName.Contains(searchTerm) || (t.Bio ?? "").Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(t => t.City == city);
        return await query.AsNoTracking().ToListAsync();
    }

    public Task<TailorProfile?> GetTailorWithServicesAsync(Guid tailorId)
        => _db.TailorProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tailorId);

    public Task<TailorProfile?> GetTailorWithPortfolioAsync(Guid tailorId)
        => _db.TailorProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tailorId);

    public Task<TailorProfile?> GetTailorWithReviewsAsync(Guid tailorId)
        => _db.TailorProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tailorId);

    public Task UpdateTailorRatingAsync(Guid tailorId)
        => Task.CompletedTask; // placeholder

    public async Task<IEnumerable<TailorProfile>> GetTopRatedTailorsAsync(int count)
        => await _db.TailorProfiles.AsNoTracking().Take(count).ToListAsync();

    public Task<TailorProfile?> GetByUserIdAsync(Guid userId)
        => _db.TailorProfiles.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId);
}
