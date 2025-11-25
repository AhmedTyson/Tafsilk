using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsPhoneUniqueAsync(string phoneNumber);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<User?> GetUserWithProfileAsync(Guid id);
        Task UpdateUserStatusAsync(Guid userId, bool isActive);
    }
}
