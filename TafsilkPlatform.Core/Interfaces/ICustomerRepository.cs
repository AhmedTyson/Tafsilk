using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<CustomerProfile>
    {
        Task<CustomerProfile?> GetByUserIdAsync(Guid userId);
        Task<CustomerProfile?> GetCustomerWithOrdersAsync(Guid customerId);
        Task<CustomerProfile?> GetCustomerWithAddressesAsync(Guid customerId);
        Task UpdateCustomerProfileAsync(Guid customerId, string fullName, string city, DateOnly? dateOfBirth);
    }
}
