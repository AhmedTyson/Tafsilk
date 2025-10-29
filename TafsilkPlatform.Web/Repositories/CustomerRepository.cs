using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Repositories;

public class CustomerRepository : EfRepository<CustomerProfile>, ICustomerRepository
{
    public CustomerRepository(AppDbContext db) : base(db) { }

    public Task<CustomerProfile?> GetByUserIdAsync(Guid userId)
        => _db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);

    public Task<CustomerProfile?> GetCustomerWithOrdersAsync(Guid customerId)
        => _db.CustomerProfiles.Include(c => c.orders).AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId);

    public Task<CustomerProfile?> GetCustomerWithAddressesAsync(Guid customerId)
        => _db.CustomerProfiles.Include(c => c.User).ThenInclude(u => u.UserAddresses).AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId);

    public async Task UpdateCustomerProfileAsync(Guid customerId, string fullName, string city, DateOnly? dateOfBirth)
    {
        var entity = await _db.CustomerProfiles.FindAsync(customerId);
        if (entity is null) return;
        entity.FullName = fullName;
        entity.City = city;
        entity.DateOfBirth = dateOfBirth;
        _db.CustomerProfiles.Update(entity);
    }
}
