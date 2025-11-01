using System.Security.Claims;

namespace TafsilkPlatform.Web.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to check user roles and claims
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Checks if user is a Tailor
    /// </summary>
    public static bool IsTailor(this ClaimsPrincipal user)
    {
     return user.IsInRole("Tailor");
    }

    /// <summary>
    /// Checks if user is a verified Tailor
    /// </summary>
    public static bool IsVerifiedTailor(this ClaimsPrincipal user)
    {
   if (!user.IsTailor())
            return false;

        var isVerifiedClaim = user.FindFirst("IsVerified");
        return isVerifiedClaim != null && 
     bool.TryParse(isVerifiedClaim.Value, out bool isVerified) && 
    isVerified;
    }

    /// <summary>
    /// Checks if user is a Customer
    /// </summary>
    public static bool IsCustomer(this ClaimsPrincipal user)
    {
        return user.IsInRole("Customer");
 }

    /// <summary>
/// Checks if user is a Corporate
    /// </summary>
    public static bool IsCorporate(this ClaimsPrincipal user)
    {
        return user.IsInRole("Corporate");
    }

    /// <summary>
    /// Checks if user is an approved Corporate
    /// </summary>
 public static bool IsApprovedCorporate(this ClaimsPrincipal user)
    {
 if (!user.IsCorporate())
       return false;

  var isApprovedClaim = user.FindFirst("IsApproved");
        return isApprovedClaim != null && 
       bool.TryParse(isApprovedClaim.Value, out bool isApproved) && 
        isApproved;
 }

    /// <summary>
    /// Checks if user is an Admin
    /// </summary>
 public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin");
  }

    /// <summary>
    /// Gets user ID from claims
 /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
{
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? 
     user.FindFirst("UserId");
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
     {
         return userId;
        }

return null;
    }

    /// <summary>
    /// Gets user email from claims
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal user)
  {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets user full name from claims
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal user)
    {
   return user.FindFirst("FullName")?.Value ?? 
    user.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
 /// Gets user role name from claims
    /// </summary>
    public static string? GetRoleName(this ClaimsPrincipal user)
    {
  return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
/// Gets company name for corporate users
    /// </summary>
  public static string? GetCompanyName(this ClaimsPrincipal user)
    {
 return user.FindFirst("CompanyName")?.Value;
    }

    /// <summary>
    /// Checks if user is a service provider (Tailor or Corporate)
    /// </summary>
    public static bool IsServiceProvider(this ClaimsPrincipal user)
    {
        return user.IsTailor() || user.IsCorporate();
    }

    /// <summary>
    /// Checks if user has any of the specified roles
 /// </summary>
    public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
    {
 return roles.Any(role => user.IsInRole(role));
    }

    /// <summary>
/// Checks if user has all of the specified roles
    /// </summary>
    public static bool IsInAllRoles(this ClaimsPrincipal user, params string[] roles)
    {
        return roles.All(role => user.IsInRole(role));
    }
}
