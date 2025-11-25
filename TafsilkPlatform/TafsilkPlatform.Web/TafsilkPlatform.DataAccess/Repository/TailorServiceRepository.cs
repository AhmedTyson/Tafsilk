using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository;

public class TailorServiceRepository : EfRepository<TailorService>, ITailorServiceRepository
{
    public TailorServiceRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<TailorService>> GetByTailorIdAsync(Guid tailorId)
        => await _db.TailorServices.Where(ts => ts.TailorId == tailorId).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<TailorService>> GetByServiceNameAsync(string serviceName)
        => await _db.TailorServices.Where(ts => ts.ServiceName == serviceName).AsNoTracking().ToListAsync();

    public Task<TailorService?> GetServiceWithPricingAsync(Guid serviceId)
        => _db.TailorServices.AsNoTracking().FirstOrDefaultAsync(ts => ts.TailorServiceId == serviceId);

    public async Task<bool> UpdateServicePricingAsync(Guid serviceId, decimal basePrice)
    {
        var entity = await _db.TailorServices.FindAsync(serviceId);
        if (entity is null) return false;
        entity.BasePrice = basePrice;
        _db.TailorServices.Update(entity);
        return true;
    }

    public async Task<IEnumerable<TailorService>> GetPopularServicesAsync(int count)
        => await _db.TailorServices.AsNoTracking().Take(count).ToListAsync();
}
