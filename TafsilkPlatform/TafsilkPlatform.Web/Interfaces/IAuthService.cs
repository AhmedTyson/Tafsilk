using System.Security.Claims;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the specified role
        /// </summary>
        Task<(bool Succeeded, string? Error, User? User)> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Validates user credentials and returns user with role information
        /// </summary>
        Task<(bool Succeeded, string? Error, User? User)> ValidateUserAsync(string email, string password);

        /// <summary>
        /// Gets user by ID with role and profile information
        /// </summary>
        Task<User?> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Gets user by email with role and profile information
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Changes user password
        /// </summary>
        Task<(bool Succeeded, string? Error)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// Checks if user has a specific role
        /// </summary>
        Task<bool> IsInRoleAsync(Guid userId, string roleName);

        /// <summary>
        /// Gets user claims for authentication
        /// </summary>
        Task<List<Claim>> GetUserClaimsAsync(User user);

        /// <summary>
        /// Activates or deactivates a user account
        /// </summary>
        Task<(bool Succeeded, string? Error)> SetUserActiveStatusAsync(Guid userId, bool isActive);

        /// <summary>
        /// Verifies a tailor profile
        /// </summary>
        Task<(bool Succeeded, string? Error)> VerifyTailorAsync(Guid tailorId, bool isVerified);

        /// <summary>
        /// Verifies user email using verification token
        /// </summary>
        Task<(bool Succeeded, string? Error)> VerifyEmailAsync(string token);

        /// <summary>
        /// Resends email verification link
        /// </summary>
        Task<(bool Succeeded, string? Error)> ResendVerificationEmailAsync(string email);
    }
}
