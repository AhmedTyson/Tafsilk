using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Repositories;
using TafsilkPlatform.Web.Security;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cookie authentication for MVC login views
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
 })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["web:client_id"] ?? throw new InvalidOperationException("Google ClientId not found");
     googleOptions.ClientSecret = builder.Configuration["web:client_secret"] ?? throw new InvalidOperationException("Google ClientSecret not found");
      googleOptions.CallbackPath = "/signin-google";
        
        // Request additional scopes if needed
        googleOptions.Scope.Add("profile");
        googleOptions.Scope.Add("email");
        
        // Save tokens for API calls if needed
     googleOptions.SaveTokens = true;
    });

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
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
