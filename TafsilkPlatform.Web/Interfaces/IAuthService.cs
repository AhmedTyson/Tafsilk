using System.Security.Claims;
using System.Threading.Tasks;
using TafsilkPlatform.Web.ViewModels;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request);
        Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password);
    }
}
