using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Helper service for user profile operations - centralized profile logic
/// Simplifies getting user names, profile pictures, and building claims
/// </summary>
public interface IUserProfileHelper
{
    /// <summary>
    /// Gets the full name for a user from their profile based on role
    /// </summary>
    Task<string> GetUserFullNameAsync(Guid userId, string? roleName = null);

    /// <summary>
    /// Gets profile picture data for a user (checks all profile types)
    /// </summary>
    Task<(byte[]? ImageData, string? ContentType)> GetProfilePictureAsync(Guid userId);

    /// <summary>
    /// Builds authentication claims for a user (includes role and full name)
    /// </summary>
    Task<List<Claim>> BuildUserClaimsAsync(User user);
}

public class UserProfileHelper : IUserProfileHelper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserProfileHelper> _logger;

    public UserProfileHelper(IUnitOfWork unitOfWork, ILogger<UserProfileHelper> logger)
    {
     _unitOfWork = unitOfWork;
   _logger = logger;
    }

    /// <summary>
    /// Gets full name from appropriate profile based on user role
    /// Returns email as fallback if no profile found
    /// </summary>
    public async Task<string> GetUserFullNameAsync(Guid userId, string? roleName = null)
    {
        try
        {
    // Get user with role if role not provided
if (string.IsNullOrEmpty(roleName))
            {
      var user = await _unitOfWork.Users.GetByIdAsync(userId);
       if (user == null) return "مستخدم";
       roleName = user.Role?.Name;
   }

            // Get name from appropriate profile
  return roleName?.ToLower() switch
            {
       "customer" => await GetCustomerNameAsync(userId),
    "tailor" => await GetTailorNameAsync(userId),
       _ => "مستخدم"
            };
        }
        catch (Exception ex)
        {
      _logger.LogWarning(ex, "Error getting full name for user {UserId}", userId);
            return "مستخدم";
        }
    }

    /// <summary>
    /// Gets profile picture from any profile type (Customer/Tailor/Corporate)
    /// Returns null if no picture found
    /// </summary>
    public async Task<(byte[]? ImageData, string? ContentType)> GetProfilePictureAsync(Guid userId)
    {
  try
        {
   // Try Customer profile first
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
    if (customer?.ProfilePictureData != null)
            {
                return (customer.ProfilePictureData, customer.ProfilePictureContentType ?? "image/jpeg");
        }

            // Try Tailor profile
       var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
     if (tailor?.ProfilePictureData != null)
            {
                return (tailor.ProfilePictureData, tailor.ProfilePictureContentType ?? "image/jpeg");
   }

     return (null, null);
        }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Error getting profile picture for user {UserId}", userId);
          return (null, null);
        }
    }

    /// <summary>
    /// Builds complete claims list for authentication cookie
    /// Includes: UserId, Email, Name, FullName, Role, and role-specific claims
    /// </summary>
    public async Task<List<Claim>> BuildUserClaimsAsync(User user)
    {
     var roleName = user.Role?.Name ?? string.Empty;
        var fullName = await GetUserFullNameAsync(user.Id, roleName);

        var claims = new List<Claim>
        {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, fullName),
            new Claim("FullName", fullName)
        };

        // Add role claim
        if (!string.IsNullOrEmpty(roleName))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
      }

      // Add role-specific claims for authorization policies
        try
        {
            await AddRoleSpecificClaimsAsync(claims, user.Id, roleName);
   }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error adding role-specific claims for user {UserId}", user.Id);
 }

      return claims;
    }

    #region Private Helper Methods

    private async Task<string> GetCustomerNameAsync(Guid userId)
 {
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        if (customer != null && !string.IsNullOrEmpty(customer.FullName))
 {
   return customer.FullName;
   }

 // Fallback to user email
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
     return user?.Email ?? "مستخدم";
    }

    private async Task<string> GetTailorNameAsync(Guid userId)
    {
        var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
        if (tailor != null && !string.IsNullOrEmpty(tailor.FullName))
        {
            return tailor.FullName;
        }

        // Fallback to user email
var user = await _unitOfWork.Users.GetByIdAsync(userId);
    return user?.Email ?? "مستخدم";
    }

    private async Task AddRoleSpecificClaimsAsync(List<Claim> claims, Guid userId, string? roleName)
    {
        switch (roleName?.ToLower())
        {
            case "tailor":
       var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(userId);
    if (tailor != null)
     {
       claims.Add(new Claim("IsVerified", tailor.IsVerified.ToString()));
      }
   break;
        }
    }

    #endregion
}
