using System.Security.Claims;

namespace TafsilkPlatform.Web.Helpers
{
    /// <summary>
    /// Helper class for role and authorization checks in Razor views
    /// Simplifies admin/tester access to all pages
    /// </summary>
    public static class RoleHelper
    {
        /// <summary>
        /// Check if user is admin or tester (has full access)
        /// </summary>
    public static bool IsAdminOrTester(ClaimsPrincipal user)
     {
            if (user?.Identity?.IsAuthenticated != true)
         return false;

       return user.IsInRole("Admin");
        }

        /// <summary>
     /// Check if user can access customer pages (Customer role OR Admin)
  /// </summary>
      public static bool CanAccessCustomerPages(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            return false;

            return user.IsInRole("Customer") || user.IsInRole("Admin");
        }

        /// <summary>
        /// Check if user can access tailor pages (Tailor role OR Admin)
        /// </summary>
        public static bool CanAccessTailorPages(ClaimsPrincipal user)
        {
    if (user?.Identity?.IsAuthenticated != true)
      return false;

   return user.IsInRole("Tailor") || user.IsInRole("Admin");
        }

        /// <summary>
        /// Check if user can access admin pages (Admin role only)
      /// </summary>
        public static bool CanAccessAdminPages(ClaimsPrincipal user)
        {
     if (user?.Identity?.IsAuthenticated != true)
 return false;

     return user.IsInRole("Admin");
}

   /// <summary>
        /// Get user's role for display purposes
/// </summary>
        public static string GetUserRole(ClaimsPrincipal user)
        {
  if (user?.Identity?.IsAuthenticated != true)
          return "Anonymous";

            if (user.IsInRole("Admin"))
           return "Admin";
            if (user.IsInRole("Tailor"))
     return "Tailor";
     if (user.IsInRole("Customer"))
        return "Customer";

            return "Unknown";
        }

        /// <summary>
        /// Get user's display name
        /// </summary>
        public static string GetUserDisplayName(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return "Guest";

          return user.FindFirst("FullName")?.Value 
                ?? user.FindFirst(ClaimTypes.Name)?.Value 
              ?? user.FindFirst(ClaimTypes.Email)?.Value 
      ?? "User";
   }

        /// <summary>
 /// Check if user is in testing mode (tester account)
        /// </summary>
   public static bool IsTesterAccount(ClaimsPrincipal user)
        {
         if (user?.Identity?.IsAuthenticated != true)
       return false;

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
   return email?.Contains("tester@", StringComparison.OrdinalIgnoreCase) == true;
      }

        /// <summary>
      /// Check if feature is accessible by current user
        /// Admins can access everything
   /// </summary>
        public static bool CanAccessFeature(ClaimsPrincipal user, string requiredRole)
        {
          if (user?.Identity?.IsAuthenticated != true)
    return false;

 // Admins can access everything
            if (user.IsInRole("Admin"))
         return true;

  // Otherwise check specific role
        return user.IsInRole(requiredRole);
     }
    }
}
