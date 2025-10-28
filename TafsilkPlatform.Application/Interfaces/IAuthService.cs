using System.Security.Claims;
using System.Threading.Tasks;
using TafsilkPlatform.Application.Models.Auth;
using TafsilkPlatform.Core.Models;

namespace TafsilkPlatform.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request);
        Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password);
    }
}
