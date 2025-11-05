using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Repositories;
using TafsilkPlatform.Web.Security;
using TafsilkPlatform.Web.Middleware;
using TafsilkPlatform.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
  {
   options.JsonSerializerOptions.PropertyNamingPolicy = null;
      options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

// Configure Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery.Tafsilk";
    options.Cookie.HttpOnly = true;
  options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
      ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDataProtection();
builder.Services.AddSession(options =>
{
 options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".Tafsilk.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
      : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ✅ AUTHENTICATION CONFIGURATION
// Base authentication with Cookie as default
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

// Cookie authentication
authBuilder.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.Name = ".Tafsilk.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
    ? CookieSecurePolicy.None 
 : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
options.ExpireTimeSpan = TimeSpan.FromDays(14);
options.SlidingExpiration = true;
});

// JWT authentication for API
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TafsilkPlatform";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TafsilkPlatformUsers";

authBuilder.AddJwtBearer("Jwt", options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = jwtIssuer,
  ValidateAudience = true,
    ValidAudience = jwtAudience,
    ValidateIssuerSigningKey = true,
     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

// ✅ GOOGLE OAUTH (if enabled)
var enableGoogleOAuth = builder.Configuration.GetValue<bool>("Features:EnableGoogleOAuth");
if (enableGoogleOAuth)
{
    var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
    var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
  {
     authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
    options.ClientId = googleClientId;
  options.ClientSecret = googleClientSecret;
   options.CallbackPath = "/signin-google";
  options.SaveTokens = true;
    
    // Request additional scopes
 options.Scope.Add("profile");
    options.Scope.Add("email");
   });
   
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
        var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
    logger.LogInformation("✅ Google OAuth configured successfully");
    }
    else
    {
      var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
        logger.LogWarning("⚠️ Google OAuth enabled but credentials not configured. Add ClientId and ClientSecret to appsettings.json");
    }
}

// Database context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
   builder.Configuration.GetConnectionString("DefaultConnection"),
   sqlOptions =>
      {
   sqlOptions.MigrationsAssembly("TafsilkPlatform.Web");
      sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
 maxRetryDelay: TimeSpan.FromSeconds(5),
      errorNumbersToAdd: null);
        });

if (builder.Environment.IsDevelopment())
    {
  options.EnableSensitiveDataLogging();
   options.EnableDetailedErrors();
 }
});

// Register repositories - Only keep what's actually used
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITailorRepository, TailorRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<ITailorServiceRepository, TailorServiceRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IRatingDimensionRepository, RatingDimensionRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services - Only essential ones
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserProfileHelper, UserProfileHelper>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileCompletionService, ProfileCompletionService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITailorRegistrationService, TailorRegistrationService>();

// Register DateTime Service
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

// Register Authorization Handlers
builder.Services.AddSingleton<IAuthorizationHandler, VerifiedTailorHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ActiveUserHandler>();

// Register TokenService
builder.Services.AddSingleton<ITokenService, TokenService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
 options.AddPolicy("AdminPolicy", policy =>
    {
     policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Admin");
  });

 options.AddPolicy("TailorPolicy", policy =>
    {
    policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
   policy.RequireRole("Tailor");
    });

    options.AddPolicy("VerifiedTailorPolicy", policy =>
    {
   policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Tailor");
    policy.RequireClaim("IsVerified", "True");
    });

    options.AddPolicy("CustomerPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
      policy.RequireRole("Customer");
    });

  options.AddPolicy("AuthenticatedPolicy", policy =>
    {
   policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
    policy.RequireAuthenticatedUser();
 });
});

var app = builder.Build();

// Initialize database in development
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync(builder.Configuration);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Check user status after authentication
app.UseMiddleware<UserStatusMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("=== Tafsilk Platform Started Successfully ===");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("Authentication Schemes: Cookies, JWT, Google");

app.Run();
