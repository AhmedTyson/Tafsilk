# Service Registration Guide

## How to Register New Services in Program.cs

### Step 1: Add Application Services

```csharp
using TafsilkPlatform.Web.Extensions;

// Replace individual service registrations with:
builder.Services.AddApplicationServices();
```

### Step 2: Add Rate Limiting (Optional but Recommended)

```csharp
builder.Services.AddRateLimiting(options =>
{
    options.MaxLoginAttempts = 5;
    options.LockoutDuration = TimeSpan.FromMinutes(15);
    options.SlidingWindow = TimeSpan.FromMinutes(5);
});
```

### Step 3: Add Middleware in Correct Order

```csharp
var app = builder.Build();

// 1. Exception handling (first)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 2. HTTPS redirection
app.UseHttpsRedirection();

// 3. Static files
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. Request logging (NEW - before authentication)
app.UseRequestLogging();

// 6. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. User status check (NEW - after authentication)
app.UseUserStatusCheck();

// 8. Endpoints
app.MapControllers();
app.MapRazorPages();

app.Run();
```

## Complete Program.cs Example

```csharp
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Extensions;
using TafsilkPlatform.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

// Add memory cache for rate limiting
builder.Services.AddMemoryCache();

// Add application services (NEW)
builder.Services.AddApplicationServices();

// Add rate limiting (NEW)
builder.Services.AddRateLimiting(options =>
{
    options.MaxLoginAttempts = 5;
    options.LockoutDuration = TimeSpan.FromMinutes(15);
});

// Add MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure middleware pipeline
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

// NEW: Add request logging
app.UseRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// NEW: Add user status check
app.UseUserStatusCheck();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
```

## Benefits of This Approach

1. **Separation of Concerns**: Each service has a single responsibility
2. **Testability**: All services can be mocked and unit tested
3. **Security**: Rate limiting and input sanitization protect against attacks
4. **Maintainability**: Extension methods keep Program.cs clean
5. **Flexibility**: Easy to add/remove services without changing controller code
6. **Performance**: Caching and compiled queries improve performance
7. **Observability**: Request logging helps with debugging and monitoring
