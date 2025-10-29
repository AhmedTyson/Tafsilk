using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class RatingDimensionRepository : EfRepository<RatingDimension>, IRatingDimensionRepository
{
    public RatingDimensionRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<RatingDimension>> GetByReviewIdAsync(Guid reviewId)
        => await _db.RatingDimensions.Where(rd => rd.ReviewId == reviewId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<RatingDimension>> GetByTailorIdAsync(Guid tailorId)
    {
        var query = from rd in _db.RatingDimensions
                    join r in _db.Reviews on rd.ReviewId equals r.ReviewId
                    where r.TailorId == tailorId
                    select rd;
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<double> GetAverageDimensionScoreAsync(Guid tailorId, string dimensionName)
    {
        var query = from rd in _db.RatingDimensions
                    join r in _db.Reviews on rd.ReviewId equals r.ReviewId
                    where r.TailorId == tailorId && rd.DimensionName == dimensionName
                    select rd.Score;
        return await query.AverageAsync(s => (double)s);
    }
}
