using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Repositories;
using TafsilkPlatform.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

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
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Check OAuth configuration
var googleClientId = builder.Configuration["Google:client_id"];
var googleClientSecret = builder.Configuration["Google:client_secret"];
var facebookAppId = builder.Configuration["Facebook:app_id"];
var facebookAppSecret = builder.Configuration["Facebook:app_secret"];

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
      options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
 
        // Request additional scopes
        googleOptions.Scope.Add("profile");
        googleOptions.Scope.Add("email");
  
        // Save tokens for API calls if needed
      googleOptions.SaveTokens = true;
        
        // Configure correlation cookie with explicit settings
        googleOptions.CorrelationCookie.Name = ".Tafsilk.Correlation.Google";
  googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
        googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
   googleOptions.CorrelationCookie.HttpOnly = true;
        googleOptions.CorrelationCookie.IsEssential = true;
        
     // Increase timeout for OAuth flow
    googleOptions.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
    });
    Console.WriteLine("✅ Google OAuth configured");
}
else
{
    Console.WriteLine("⚠️  WARNING: Google OAuth not configured - social login will not work");
    Console.WriteLine(" Configure: dotnet user-secrets set \"Google:client_id\" \"YOUR_ID\"");
    Console.WriteLine("   Configure: dotnet user-secrets set \"Google:client_secret\" \"YOUR_SECRET\"");
}

// Conditionally add Facebook OAuth only if configured
if (facebookConfigured)
{
  authBuilder.AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = facebookAppId!;
        facebookOptions.AppSecret = facebookAppSecret!;
        facebookOptions.CallbackPath = "/signin-facebook";
        
        // Scope for permissions - email and public_profile are included by default
        facebookOptions.Scope.Add("email");
        facebookOptions.Scope.Add("public_profile");
        
        // Fields to retrieve from Facebook Graph API
        facebookOptions.Fields.Add("name");
        facebookOptions.Fields.Add("email");
        facebookOptions.Fields.Add("picture");
        
     // Save tokens for API calls if needed
    facebookOptions.SaveTokens = true;

        // Configure correlation cookie with explicit settings
        facebookOptions.CorrelationCookie.Name = ".Tafsilk.Correlation.Facebook";
        facebookOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
        facebookOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        facebookOptions.CorrelationCookie.HttpOnly = true;
        facebookOptions.CorrelationCookie.IsEssential = true;
        
      // Increase timeout for OAuth flow
      facebookOptions.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
    });
    Console.WriteLine("✅ Facebook OAuth configured");
}
else
{
    Console.WriteLine("⚠️  WARNING: Facebook OAuth not configured - social login will not work");
    Console.WriteLine("   Configure: dotnet user-secrets set \"Facebook:app_id\" \"YOUR_ID\"");
    Console.WriteLine("   Configure: dotnet user-secrets set \"Facebook:app_secret\" \"YOUR_SECRET\"");
}

// Database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
  b => b.MigrationsAssembly("TafsilkPlatform.Web")));

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

var app = builder.Build();

// Validate configuration and database on startup
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
      // Check database connectivity
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var canConnect = await dbContext.Database.CanConnectAsync();
  
            if (canConnect)
         {
       logger.LogInformation("✅ Database connection successful");
        
    // Check for pending migrations
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
         {
      logger.LogWarning("⚠️  Pending migrations found: {Migrations}", 
          string.Join(", ", pendingMigrations));
      logger.LogWarning("   Run: dotnet ef database update");
       }
       else
  {
         logger.LogInformation("✅ Database schema is up to date");
                }
       }
          else
            {
        logger.LogError("❌ Cannot connect to database");
    }
        }
        catch (Exception ex)
        {
        logger.LogError(ex, "❌ Database validation failed");
    }
 
        // OAuth configuration summary
        if (googleConfigured)
  logger.LogInformation("✅ Google OAuth configured");
        else
  logger.LogWarning("⚠️  Google OAuth not configured - social login disabled");
        
        if (facebookConfigured)
            logger.LogInformation("✅ Facebook OAuth configured");
        else
logger.LogWarning("⚠️  Facebook OAuth not configured - social login disabled");
        
        if (!googleConfigured || !facebookConfigured)
    {
            logger.LogWarning("💡 To enable social login, configure OAuth secrets using user-secrets");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware BEFORE authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Log startup success
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("🚀 Tafsilk Platform started successfully");
startupLogger.LogInformation("📍 Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation("🌐 Application URLs: {Urls}", string.Join(", ", app.Urls));

app.Run();
