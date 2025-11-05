using Microsoft.AspNetCore.Authorization;

namespace TafsilkPlatform.Web.Security;

/// <summary>
/// Requires authenticated user with Tailor role
/// </summary>
public class AuthorizeTailorAttribute : AuthorizeAttribute
{
    public AuthorizeTailorAttribute()
    {
        Policy = "TailorPolicy";
    }
}

/// <summary>
/// Requires authenticated user with Tailor role and verified status
/// </summary>
public class AuthorizeVerifiedTailorAttribute : AuthorizeAttribute
{
    public AuthorizeVerifiedTailorAttribute()
    {
        Policy = "VerifiedTailorPolicy";
    }
}

/// <summary>
/// Requires authenticated user with Customer role
/// </summary>
public class AuthorizeCustomerAttribute : AuthorizeAttribute
{
    public AuthorizeCustomerAttribute()
    {
        Policy = "CustomerPolicy";
    }
}

/// <summary>
/// Requires authenticated user with Corporate role
/// </summary>
public class AuthorizeCorporateAttribute : AuthorizeAttribute
{
    public AuthorizeCorporateAttribute()
    {
        Policy = "CorporatePolicy";
    }
}

/// <summary>
/// Requires authenticated user with Corporate role and approved status
/// </summary>
public class AuthorizeApprovedCorporateAttribute : AuthorizeAttribute
{
    public AuthorizeApprovedCorporateAttribute()
    {
        Policy = "ApprovedCorporatePolicy";
    }
}

/// <summary>
/// Requires authenticated user with Admin role
/// </summary>
public class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute()
    {
        Policy = "AdminPolicy";
    }
}

/// <summary>
/// Requires authenticated user with Customer or Tailor role
/// </summary>
public class AuthorizeCustomerOrTailorAttribute : AuthorizeAttribute
{
    public AuthorizeCustomerOrTailorAttribute()
    {
        Policy = "CustomerOrTailorPolicy";
    }
}

/// <summary>
/// Requires authenticated user with Tailor or Corporate role (service providers)
/// </summary>
public class AuthorizeServiceProviderAttribute : AuthorizeAttribute
{
    public AuthorizeServiceProviderAttribute()
    {
        Policy = "ServiceProviderPolicy";
    }
}
