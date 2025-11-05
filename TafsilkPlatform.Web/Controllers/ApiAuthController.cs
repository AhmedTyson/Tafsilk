using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.ViewModels;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// API Authentication Controller - Handles JWT-based authentication for mobile/SPA clients
/// </summary>
[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<ApiAuthController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ApiAuthController(
        IAuthService authService,
        IConfiguration config,
        ILogger<ApiAuthController> logger,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _config = config;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Register a new user (Customer/Corporate only via API)
    /// Tailors MUST use web interface for evidence submission
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "بيانات التسجيل غير صالحة", // Invalid registration data
                errors = errors
            });
        }

        // Block Tailor registration via API - must use web for evidence
        if (request.Role == RegistrationRole.Tailor)
        {
            return BadRequest(new
            {
                success = false,
                message = "تسجيل الخياطين يجب أن يتم عبر الموقع لتقديم الأوراق الثبوتية", // Tailor registration must be done via website for evidence submission
                redirectUrl = "/Account/Register"
            });
        }

        var (Succeeded, Error, User) = await _authService.RegisterAsync(request);

        if (!Succeeded)
        {
            _logger.LogWarning("API Registration failed for {Email}: {Error}", request.Email, Error);

            return BadRequest(new
            {
                success = false,
                message = GetArabicErrorMessage(Error),
                error = Error
            });
        }

        _logger.LogInformation("API Registration successful for {Email}, Role: {Role}", request.Email, request.Role);

        return Ok(new
        {
            success = true,
            message = "تم إنشاء الحساب بنجاح. يرجى تسجيل الدخول", // Account created successfully. Please login
            userId = User?.Id
        });
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "بيانات تسجيل الدخول غير صالحة", // Invalid login data
                errors = errors
            });
        }

        var (Succeeded, Error, User) = await _authService.ValidateUserAsync(model.Email, model.Password);

        if (!Succeeded || User is null)
        {
            _logger.LogWarning("API Login failed for {Email}: {Error}", model.Email, Error);

            // Handle specific tailor errors
            if (Error == "TAILOR_INCOMPLETE_PROFILE")
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "يجب تقديم الأوراق الثبوتية لإكمال التسجيل قبل تسجيل الدخول", // Must provide evidence to complete registration before login
                    requiresEvidence = true,
                    redirectUrl = "/Account/CompleteTailorProfile", // ← CHANGED: Better UX page
                    userId = User?.Id
                });
            }

            return Unauthorized(new
            {
                success = false,
                message = GetArabicErrorMessage(Error) ?? "البريد الإلكتروني أو كلمة المرور غير صحيحة", // Email or password is incorrect
                error = Error
            });
        }

        // Check if user account is active
        if (!User.IsActive)
        {
            var roleName = User.Role?.Name?.ToLower();
            string message = roleName switch
            {
                "tailor" => "حسابك قيد المراجعة من قبل الإدارة. سيتم إشعارك عند الموافقة", // Your account is under admin review. You'll be notified upon approval
                _ => "حسابك غير نشط. يرجى الاتصال بالدعم" // Your account is inactive. Please contact support
            };

            return Unauthorized(new
            {
                success = false,
                message = message,
                isPending = true,
                role = roleName
            });
        }

        // Generate JWT token
        var tokenResponse = GenerateJwtToken(User);

        _logger.LogInformation("API Login successful for {Email}, Role: {Role}", User.Email, User.Role?.Name);

        return Ok(new
        {
            success = true,
            message = "تم تسجيل الدخول بنجاح", // Login successful
            token = tokenResponse.Token,
            expiresAt = tokenResponse.ExpiresAtUtc,
            user = new
            {
                id = User.Id,
                email = User.Email,
                role = User.Role?.Name,
                isActive = User.IsActive
            }
        });
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(new
            {
                success = false,
                message = "جلسة غير صالحة. يرجى تسجيل الدخول مرة أخرى" // Invalid session. Please login again
            });
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userGuid);

        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "المستخدم غير موجود" // User not found
            });
        }

        var roleName = user.Role?.Name?.ToLower();
        object? profileData = null;

        // Get role-specific profile data
        switch (roleName)
        {
            case "customer":
                var customer = await _unitOfWork.Customers.GetByUserIdAsync(userGuid);
                profileData = customer != null ? new
                {
                    fullName = customer.FullName,
                    city = customer.City,
                    gender = customer.Gender,
                    dateOfBirth = customer.DateOfBirth
                } : null;
                break;

            case "tailor":
                var tailor = await _unitOfWork.Tailors.GetByUserIdAsync(userGuid);
                profileData = tailor != null ? new
                {
                    fullName = tailor.FullName,
                    shopName = tailor.ShopName,
                    city = tailor.City,
                    isVerified = tailor.IsVerified,
                    averageRating = tailor.AverageRating,
                    experienceYears = tailor.ExperienceYears
                } : null;
                break;
        }

        return Ok(new
        {
            success = true,
            user = new
            {
                id = user.Id,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                role = roleName,
                isActive = user.IsActive,
                createdAt = user.CreatedAt,
                profile = profileData
            }
        });
    }

    /// <summary>
    /// Refresh JWT token (future implementation with refresh tokens)
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // TODO: Implement refresh token logic
        return BadRequest(new
        {
            success = false,
            message = "تحديث الرمز غير مدعوم حالياً" // Token refresh not currently supported
        });
    }

    /// <summary>
    /// Logout (invalidate token - client-side mostly)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("API Logout for user: {UserId}", userId);

        // TODO: Blacklist token if implementing token blacklist

        return Ok(new
        {
            success = true,
            message = "تم تسجيل الخروج بنجاح" // Logout successful
        });
    }

    #region Private Helper Methods

    private TokenResponse GenerateJwtToken(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var issuer = _config["Jwt:Issuer"] ?? "TafsilkPlatform";
        var audience = _config["Jwt:Audience"] ?? "TafsilkPlatformUsers";
        var expirationMinutes = _config.GetValue<int>("Jwt:ExpirationMinutes", 60);
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add role claim
        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            claims.Add(new Claim("role", user.Role.Name)); // Duplicate for compatibility
        }

        // Add verification status for tailors
        if (user.Role?.Name?.ToLower() == "tailor" && user.TailorProfile != null)
        {
            claims.Add(new Claim("IsVerified", user.TailorProfile.IsVerified.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse
        {
            Token = tokenString,
            ExpiresAtUtc = expires,
            Role = user.Role?.Name ?? string.Empty
        };
    }

    private string GetArabicErrorMessage(string? error)
    {
        if (string.IsNullOrEmpty(error)) return "حدث خطأ غير متوقع"; // Unexpected error occurred

        return error switch
        {
            "TAILOR_INCOMPLETE_PROFILE" => "يجب تقديم الأوراق الثبوتية لإكمال التسجيل",
            "USER_NOT_ACTIVE" => "حسابك غير نشط. يرجى الاتصال بالدعم",
            "INVALID_CREDENTIALS" => "البريد الإلكتروني أو كلمة المرور غير صحيحة",
            "EMAIL_ALREADY_EXISTS" => "البريد الإلكتروني مسجل بالفعل",
            "WEAK_PASSWORD" => "كلمة المرور ضعيفة جداً",
            "TAILOR_PENDING_VERIFICATION" => "حسابك قيد المراجعة من قبل الإدارة",
            "CORPORATE_PENDING_APPROVAL" => "حسابك الشركي قيد المراجعة من قبل الإدارة",
            "REGISTRATION_FAILED" => "فشل التسجيل. يرجى المحاولة مرة أخرى",
            _ => error.Contains("exists", StringComparison.OrdinalIgnoreCase)
                ? "البريد الإلكتروني مسجل بالفعل"
                : "حدث خطأ. يرجى المحاولة مرة أخرى"
        };
    }

    #endregion
}

/// <summary>
/// Request model for token refresh (future use)
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
