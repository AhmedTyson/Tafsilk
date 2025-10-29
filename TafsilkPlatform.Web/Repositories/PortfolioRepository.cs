using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class PortfolioRepository : EfRepository<PortfolioImage>, IPortfolioRepository
{
    public PortfolioRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<PortfolioImage>> GetByTailorIdAsync(Guid tailorId)
        => await _db.PortfolioImages.Where(p => p.TailorId == tailorId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<PortfolioImage>> GetBeforeAfterImagesAsync(Guid tailorId)
        => await _db.PortfolioImages.Where(p => p.TailorId == tailorId && p.IsBeforeAfter).AsNoTracking().ToListAsync();

    public async Task<bool> AddPortfolioImagesAsync(Guid tailorId, IEnumerable<string> imageUrls, bool isBeforeAfter)
    {
        var images = imageUrls.Select(url => new PortfolioImage { PortfolioImageId = Guid.NewGuid(), TailorId = tailorId, ImageUrl = url, IsBeforeAfter = isBeforeAfter, UploadedAt = DateTime.UtcNow });
        await _db.PortfolioImages.AddRangeAsync(images);
        return true;
    }

    public async Task<bool> RemovePortfolioImageAsync(Guid imageId)
    {
        var entity = await _db.PortfolioImages.FindAsync(imageId);
        if (entity is null) return false;
        _db.PortfolioImages.Remove(entity);
        return true;
    }

    public async Task<int> GetPortfolioCountAsync(Guid tailorId)
        => await _db.PortfolioImages.CountAsync(p => p.TailorId == tailorId);
}
