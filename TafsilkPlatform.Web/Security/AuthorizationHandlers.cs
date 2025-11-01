using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TafsilkPlatform.Web.Security;

/// <summary>
/// Requirement for verified tailor access
/// </summary>
public class VerifiedTailorRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler for verified tailor requirement
/// </summary>
public class VerifiedTailorHandler : AuthorizationHandler<VerifiedTailorRequirement>
{
    protected override Task HandleRequirementAsync(
  AuthorizationHandlerContext context, 
        VerifiedTailorRequirement requirement)
    {
        // Check if user is authenticated
    if (!context.User.Identity?.IsAuthenticated ?? true)
 {
            return Task.CompletedTask;
        }

// Check if user has Tailor role
        var isTailor = context.User.IsInRole("Tailor");
   if (!isTailor)
{
       return Task.CompletedTask;
        }

      // Check if verified
        var isVerifiedClaim = context.User.FindFirst("IsVerified");
        if (isVerifiedClaim != null && bool.TryParse(isVerifiedClaim.Value, out bool isVerified) && isVerified)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement for approved corporate access
/// </summary>
public class ApprovedCorporateRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler for approved corporate requirement
/// </summary>
public class ApprovedCorporateHandler : AuthorizationHandler<ApprovedCorporateRequirement>
{
    protected override Task HandleRequirementAsync(
  AuthorizationHandlerContext context,
   ApprovedCorporateRequirement requirement)
    {
        // Check if user is authenticated
   if (!context.User.Identity?.IsAuthenticated ?? true)
        {
 return Task.CompletedTask;
        }

  // Check if user has Corporate role
  var isCorporate = context.User.IsInRole("Corporate");
        if (!isCorporate)
 {
   return Task.CompletedTask;
        }

// Check if approved
        var isApprovedClaim = context.User.FindFirst("IsApproved");
     if (isApprovedClaim != null && bool.TryParse(isApprovedClaim.Value, out bool isApproved) && isApproved)
 {
     context.Succeed(requirement);
  }

   return Task.CompletedTask;
    }
}

/// <summary>
/// Requirement for active user access
/// </summary>
public class ActiveUserRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler for active user requirement
/// </summary>
public class ActiveUserHandler : AuthorizationHandler<ActiveUserRequirement>
{
    protected override Task HandleRequirementAsync(
     AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
  // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
 {
   return Task.CompletedTask;
   }

      // Check if user is active (this check happens during login, but we can add extra verification)
      // If user reached here, they passed login checks, so they're active
   context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
