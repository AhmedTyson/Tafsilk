using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Text;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Repositories;
using TafsilkPlatform.Web.Security;
using TafsilkPlatform.Web.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging with Serilog-ready structure
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
     options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase
   options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
 });

// Add API Explorer for Swagger
builder.Services.AddEndpointsApiExplorer();

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
  Title = "Tafsilk Platform API",
    Version = "v1.0",
      Description = "API for Tafsilk - Tailoring Marketplace Platform",
        Contact = new OpenApiContact
   {
      Name = "Tafsilk Support",
            Email = "support@tafsilk.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
     {
            new OpenApiSecurityScheme
   {
   Reference = new OpenApiReference
              {
     Type = ReferenceType.SecurityScheme,
      Id = "Bearer"
          }
  },
            Array.Empty<string>()
        }
    });
});

// Configure Antiforgery to work with HTTP in development
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery.Tafsilk";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add distributed memory cache for better state persistence
builder.Services.AddDistributedMemoryCache();

// Configure data protection for OAuth state persistence
builder.Services.AddDataProtection();

// Configure session for OAuth state with distributed cache
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

// Check OAuth configuration - prefer user secrets in development
var googleClientId = builder.Configuration["Google:client_id"];
var googleClientSecret = builder.Configuration["Google:client_secret"];
var facebookAppId = builder.Configuration["Facebook:app_id"];
var facebookAppSecret = builder.Configuration["Facebook:app_secret"];

// Log configuration status for debugging
var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
logger.LogInformation("=== OAuth Configuration Status ===");
logger.LogInformation("Google OAuth: {Status}", !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret) ? "Configured" : "Not Configured");
logger.LogInformation("Facebook OAuth: {Status}", !string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret) ? "Configured" : "Not Configured");

var googleConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);
var facebookConfigured = !string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret);

// Cookie authentication for MVC login views with enhanced configuration
var authBuilder = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
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

// Conditionally add Google OAuth only if configured
if (googleConfigured)
{
    authBuilder.AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClientId!;
      googleOptions.ClientSecret = googleClientSecret!;
        googleOptions.CallbackPath = "/signin-google";
        
     googleOptions.Scope.Add("profile");
  googleOptions.Scope.Add("email");
    googleOptions.SaveTokens = true;
        
        googleOptions.CorrelationCookie.Name = ".Tafsilk.Correlation.Google";
        googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
    googleOptions.CorrelationCookie.SecurePolicy = builder.Environment.IsDevelopment()
         ? CookieSecurePolicy.None
: CookieSecurePolicy.Always;
  googleOptions.CorrelationCookie.HttpOnly = true;
        googleOptions.CorrelationCookie.IsEssential = true;
     googleOptions.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
    });
    logger.LogInformation("✓ Google OAuth provider registered");
}
else
{
    logger.LogWarning("⚠ Google OAuth not configured - set user secrets to enable");
}

// Conditionally add Facebook OAuth only if configured
if (facebookConfigured)
{
    authBuilder.AddFacebook(facebookOptions =>
{
  facebookOptions.AppId = facebookAppId!;
        facebookOptions.AppSecret = facebookAppSecret!;
    facebookOptions.CallbackPath = "/signin-facebook";
        
        facebookOptions.Scope.Add("email");
        facebookOptions.Scope.Add("public_profile");
        facebookOptions.Fields.Add("name");
        facebookOptions.Fields.Add("email");
      facebookOptions.Fields.Add("picture");
    facebookOptions.SaveTokens = true;

    facebookOptions.CorrelationCookie.Name = ".Tafsilk.Correlation.Facebook";
        facebookOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
        facebookOptions.CorrelationCookie.SecurePolicy = builder.Environment.IsDevelopment()
   ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always;
        facebookOptions.CorrelationCookie.HttpOnly = true;
  facebookOptions.CorrelationCookie.IsEssential = true;
        facebookOptions.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
  });
    logger.LogInformation("✓ Facebook OAuth provider registered");
}
else
{
    logger.LogWarning("⚠ Facebook OAuth not configured - set user secrets to enable");
}

// Database context with retry on failure
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
  {
        sqlOptions.MigrationsAssembly("TafsilkPlatform.Web");
            sqlOptions.EnableRetryOnFailure(
       maxRetryCount: 3,
             maxRetryDelay: TimeSpan.FromSeconds(5),
      errorNumbersToAdd: null);
        }));

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        tags: new[] { "ready", "db" });

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITailorRepository, TailorRepository>();
builder.Services.AddScoped<ICorporateRepository, CorporateRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IRFQRepository, RFQRepository>();
builder.Services.AddScoped<IRFQBidRepository, RFQBidRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<ITailorServiceRepository, TailorServiceRepository>();
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IRatingDimensionRepository, RatingDimensionRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileCompletionService, ProfileCompletionService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Register DateTime Service for Egypt timezone
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

// Register Authorization Handlers
builder.Services.AddSingleton<IAuthorizationHandler, VerifiedTailorHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ApprovedCorporateHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ActiveUserHandler>();

// Register TokenService
builder.Services.AddSingleton<ITokenService, TokenService>();

// Configure JWT authentication as an additional scheme
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TafsilkPlatform";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TafsilkPlatformUsers";

builder.Services.AddAuthentication()
    .AddJwtBearer("Jwt", options =>
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

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Admin");
    });

    options.AddPolicy("AdminApiPolicy", policy =>
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

    options.AddPolicy("CorporatePolicy", policy =>
    {
   policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Corporate");
    });

  options.AddPolicy("ApprovedCorporatePolicy", policy =>
    {
policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Corporate");
        policy.RequireClaim("IsApproved", "True");
    });

    options.AddPolicy("AuthenticatedPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("CustomerOrTailorPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Customer", "Tailor");
    });

    options.AddPolicy("ServiceProviderPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireRole("Tailor", "Corporate");
  });
});

var app = builder.Build();

// Seed admin role and user during development if missing
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
     try
        {
      db.Database.Migrate();
   TafsilkPlatform.Web.Data.Seed.AdminSeeder.Seed(db, builder.Configuration, seedLogger);
        }
        catch (InvalidOperationException ex) when (
  ex.Message?.Contains("pending", StringComparison.OrdinalIgnoreCase) == true ||
            ex.Message?.Contains("PendingModelChangesWarning", StringComparison.OrdinalIgnoreCase) == true)
        {
      seedLogger.LogWarning(ex, "Automatic database migration skipped - pending model changes detected. Create migration with: dotnet ef migrations add <Name>");
        }
        catch (Exception ex)
        {
    seedLogger.LogError(ex, "Failed to run admin seeder");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tafsilk Platform API V1");
        c.RoutePrefix = "swagger";
 });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enable response compression
app.UseResponseCompression();

app.UseStaticFiles();

app.UseRouting();

// Add session middleware BEFORE authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Check user status after authentication
app.UseMiddleware<UserStatusMiddleware>();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log startup success
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("=== Tafsilk Platform Started Successfully ===");
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("URLs: {Urls}", string.Join(", ", app.Urls));
startupLogger.LogInformation("Health Check: /health");
if (app.Environment.IsDevelopment())
{
    startupLogger.LogInformation("Swagger UI: /swagger");
}

app.Run();
