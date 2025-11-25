using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Text;
using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Web.Controllers; // For IdempotencyCleanupService
using TafsilkPlatform.Web.Middleware;
using TafsilkPlatform.Web.Services;
using Microsoft.AspNetCore.Hosting;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Tafsilk Platform application");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ SIMPLE CONFIGURATION VALIDATION - Clear error messages if something is missing
    try
    {
        TafsilkPlatform.Utility.Helpers.ConfigurationHelper.ValidateRequiredConfiguration(
            builder.Configuration,
            LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup"));
    }
    catch (InvalidOperationException ex)
    {
        Log.Fatal(ex, "Configuration validation failed. Please fix the issues above.");
        throw;
    }

    // Use Serilog for logging
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Configure logging (fallback if Serilog fails)
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

    // ✅ SWAGGER/OPENAPI CONFIGURATION
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Tafsilk Platform API",
            Description = "Tafsilk - منصة الخياطين والتفصيل - API Documentation",
            Contact = new OpenApiContact
            {
                Name = "Tafsilk Platform",
                Email = "support@tafsilk.com",
                Url = new Uri("https://tafsilk.com")
            },
            License = new OpenApiLicense
            {
                Name = "Use under Tafsilk License",
                Url = new Uri("https://tafsilk.com/license")
            }
        });

        // Add JWT Authentication
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
        });

        // Add Cookie Authentication
        options.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
        {
            Name = ".Tafsilk.Auth",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Description = "Cookie-based authentication"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        // Include XML comments if available
        var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
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

    // ✅ JWT authentication - SIMPLIFIED: Auto-detects from User Secrets or Environment
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? Environment.GetEnvironmentVariable("JWT_KEY")
        ?? throw new InvalidOperationException(
            "JWT Key is required. " +
            "Quick fix: dotnet user-secrets set \"Jwt:Key\" \"YourKeyHere\" " +
            "(must be at least 32 characters)");

    if (jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            $"JWT Key is too short ({jwtKey.Length} chars). " +
            "Must be at least 32 characters for security.");
    }

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
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        // If connection string points to a SQLite file (development), use SQLite provider
        if (!string.IsNullOrEmpty(connectionString) && connectionString.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlite(connectionString, sqliteOpt => sqliteOpt.MigrationsAssembly("TafsilkPlatform.DataAccess"));
        }
        else
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("TafsilkPlatform.DataAccess");
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
        }

        // Enable sensitive data logging only when explicitly allowed in config AND in Development
        var enableSensitive = builder.Configuration.GetValue<bool>("Database:EnableSensitiveDataLogging", false);
        if (builder.Environment.IsDevelopment() && enableSensitive)
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
        else if (builder.Environment.IsDevelopment())
        {
            // Still allow detailed errors in development without sensitive data
            options.EnableDetailedErrors();
        }
    });

    // Register repositories - Only keep what's actually used
    builder.Services.AddScoped(typeof(TafsilkPlatform.DataAccess.Repository.IRepository<>), typeof(TafsilkPlatform.DataAccess.Repository.EfRepository<>));
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IUserRepository, TafsilkPlatform.DataAccess.Repository.UserRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.ICustomerRepository, TafsilkPlatform.DataAccess.Repository.CustomerRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.ITailorRepository, TafsilkPlatform.DataAccess.Repository.TailorRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IOrderRepository, TafsilkPlatform.DataAccess.Repository.OrderRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IOrderItemRepository, TafsilkPlatform.DataAccess.Repository.OrderItemRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IPaymentRepository, TafsilkPlatform.DataAccess.Repository.PaymentRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IPortfolioRepository, TafsilkPlatform.DataAccess.Repository.PortfolioRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.ITailorServiceRepository, TafsilkPlatform.DataAccess.Repository.TailorServiceRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IAddressRepository, TafsilkPlatform.DataAccess.Repository.AddressRepository>();
    builder.Services.AddScoped<TafsilkPlatform.Web.Interfaces.IOrderService, OrderService>();
    // ✅ ECOMMERCE: Register product and cart repositories
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IProductRepository, TafsilkPlatform.DataAccess.Repository.ProductRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IShoppingCartRepository, TafsilkPlatform.DataAccess.Repository.ShoppingCartRepository>();
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.ICartItemRepository, TafsilkPlatform.DataAccess.Repository.CartItemRepository>();

    // Register Unit of Work
    builder.Services.AddScoped<TafsilkPlatform.DataAccess.Repository.IUnitOfWork, TafsilkPlatform.DataAccess.Repository.UnitOfWork>();

    // Register services - Only essential ones
    builder.Services.AddScoped<TafsilkPlatform.Web.Interfaces.IAuthService, AuthService>();
    builder.Services.AddScoped<IUserProfileHelper, UserProfileHelper>();
    builder.Services.AddScoped<IFileUploadService, FileUploadService>();
    builder.Services.AddScoped<ImageUploadService>(); // Best practices image upload service
    // Register AttachmentService (file storage helper)
    builder.Services.AddScoped<BLL.Services.Interfaces.IAttachmentService, BLL.Services.AttachmentService>();
    builder.Services.AddScoped<TafsilkPlatform.Utility.IEmailService, TafsilkPlatform.Utility.EmailService>();
    builder.Services.AddScoped<IProfileCompletionService, ProfileCompletionService>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IValidationService, ValidationService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<ITailorRegistrationService, TailorRegistrationService>();
    // ✅ IDEMPOTENCY: Register IdempotencyStore for preventing duplicate requests
    builder.Services.AddScoped<IIdempotencyStore, EfCoreIdempotencyStore>();
    // Register background service for cleaning up expired idempotency keys
    builder.Services.AddHostedService<IdempotencyCleanupService>();
    // Register CacheService (using in-memory cache for single-instance deployments)
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();

    // ✅ GLOBAL EXCEPTION HANDLER - Catches all unhandled exceptions
    builder.Services.AddGlobalExceptionHandler();

    // ✅ ECOMMERCE: Register store service
    builder.Services.AddScoped<TafsilkPlatform.Web.Interfaces.IStoreService, StoreService>();
    // Register product and portfolio management services
    builder.Services.AddScoped<TafsilkPlatform.Web.Services.IProductManagementService, TafsilkPlatform.Web.Services.ProductManagementService>();
    builder.Services.AddScoped<TafsilkPlatform.Web.Services.IPortfolioService, TafsilkPlatform.Web.Services.PortfolioService>();
    // ✅ PAYMENT: Register payment processor service (supports Cash + Stripe when configured)
    builder.Services.AddScoped<TafsilkPlatform.Web.Services.Payment.IPaymentProcessorService, TafsilkPlatform.Web.Services.Payment.PaymentProcessorService>();


    // Register DateTime Service
    builder.Services.AddSingleton<IDateTimeService, DateTimeService>();

    // Register Authorization Handlers
    builder.Services.AddSingleton<IAuthorizationHandler, TafsilkPlatform.Web.Security.VerifiedTailorHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, TafsilkPlatform.Web.Security.ActiveUserHandler>();

    // Register TokenService
    builder.Services.AddSingleton<TafsilkPlatform.Web.Security.ITokenService, TafsilkPlatform.Web.Security.TokenService>();

    // ✅ RESPONSE COMPRESSION (if enabled in config)
    var enableResponseCompression = builder.Configuration.GetValue<bool>("Performance:EnableResponseCompression", true);
    if (enableResponseCompression)
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
            "application/json",
            "application/javascript",
            "text/css",
            "text/html",
            "text/plain",
            "text/xml",
            "application/xml",
            "image/svg+xml"
            });
        });

        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });
    }

    // ✅ CORS Configuration (for API access)
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // ✅ HEALTH CHECKS
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "ready" })
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "self" })
        .AddCheck<TafsilkPlatform.Web.HealthChecks.AttachmentHealthCheck>("attachments", tags: new[] { "ready", "attachments" });

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

    // Configure maximum upload sizes (Kestrel + FormOptions)
    // Default max upload size: 50 MB (can be overridden via configuration: Uploads:MaxRequestBodySizeBytes)
    var defaultMaxUploadBytes = builder.Configuration.GetValue<long?>("Uploads:MaxRequestBodySizeBytes") ?? 50 * 1024 * 1024;

    // Kestrel server limit
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB
    });

    // Multipart form options (for model binding IFormFile)
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = defaultMaxUploadBytes; // bytes
        options.MultipartHeadersLengthLimit = 16384; // header size limit
        options.ValueLengthLimit = int.MaxValue;
        options.MemoryBufferThreshold = 1024 * 1024; // 1MB before buffering to disk
    });

    var app = builder.Build();

    // Fallback error-handling middleware (last in pipeline)
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Unhandled exception in fallback middleware");
            context.Response.Redirect("/Error?message=حدث خطأ غير متوقع");
        }
    });

    // Initialize database in development
    // Note: Database seeding can be done via migrations or a separate initialization service
    // if (app.Environment.IsDevelopment())
    // {
    //     // Database initialization logic can be added here if needed
    // }

    // ✅ SECURITY HEADERS MIDDLEWARE (must be early in pipeline)
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // ✅ GLOBAL EXCEPTION HANDLER (must be early in pipeline)
    app.UseExceptionHandler("/Error");

    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        // In development, use detailed error page for better debugging
        app.UseDeveloperExceptionPage();

        // ✅ SWAGGER MIDDLEWARE
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tafsilk Platform API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Tafsilk Platform API";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableTryItOutByDefault();
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        app.UseHttpsRedirection(); // Force HTTPS in production
    }

    // ✅ CORS (must be before UseRouting)
    app.UseCors();

    // ✅ RESPONSE COMPRESSION (must be before UseStaticFiles and UseRouting)
    var enableResponseCompressionMiddleware = app.Configuration.GetValue<bool>("Performance:EnableResponseCompression", true);
    if (enableResponseCompressionMiddleware && !app.Environment.IsDevelopment())
    {
        app.UseResponseCompression();
        Log.Information("✅ Response compression enabled (Production mode)");
    }
    else if (app.Environment.IsDevelopment())
    {
        Log.Information("ℹ️ Response compression disabled in Development mode for better debugging");
    }

    // ✅ HTTPS Redirection (only if not already added in production)
    if (!app.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
    }

    app.UseStaticFiles();
    app.UseRouting();

    // ✅ HEALTH CHECKS ENDPOINT
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self")
    });

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // Check user status after authentication
    app.UseMiddleware<UserStatusMiddleware>();

    // ✅ MAP CONTROLLERS - Required for API endpoint discovery
    app.MapControllers();

    // Area routing (must come before default route)
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    // Default route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");


    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
    Log.Information("=== Tafsilk Platform Started Successfully ===");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Authentication Schemes: Cookies, JWT, Google");

    if (app.Environment.IsDevelopment())
    {
        var urls = app.Urls;
        if (urls.Any())
        {
            foreach (var url in urls)
            {
                Log.Information("🔷 Swagger UI available at: {SwaggerUrl}", $"{url}/swagger");
                Log.Information("🔷 Swagger JSON available at: {SwaggerJsonUrl}", $"{url}/swagger/v1/swagger.json");
            }
        }
        else
        {
            // Fallback to common development URLs
            Log.Information("🔷 Swagger UI available at: https://localhost:7186/swagger");
            Log.Information("🔷 Swagger UI available at: http://localhost:5140/swagger");
        }

        Log.Information("🔷 Health Check available at: /health");
        Log.Information("🔷 Health Check (Ready) available at: /health/ready");
        Log.Information("🔷 Health Check (Live) available at: /health/live");
    }

    // ✅ Verify database connection before accepting requests
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var providerName = db.Database.ProviderName ?? string.Empty;

        // Apply migrations and seed minimal data
        // If using SQLite in development, prefer EnsureCreated to avoid running SQL Server-specific migrations
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                await db.Database.EnsureCreatedAsync();
                Log.Information("✓ SQLite database ensured/created successfully");

                if (!await db.Roles.AnyAsync())
                {
                    db.Roles.AddRange(
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator" , CreatedAt = DateTime.UtcNow},
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Tailor", Description = "Tailor role" , CreatedAt = DateTime.UtcNow},
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role" , CreatedAt = DateTime.UtcNow}
                    );
                    await db.SaveChangesAsync();
                    Log.Information("✓ Seeded default roles into SQLite database");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to ensure/create SQLite database during startup");
                if (!app.Environment.IsDevelopment()) throw;
            }
        }
        else
        {
            // For SQL Server and other providers, attempt to apply migrations
            try
            {
                await db.Database.MigrateAsync();
                Log.Information("✓ Database migrations applied successfully");

                if (!await db.Roles.AnyAsync())
                {
                    db.Roles.AddRange(
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator" , CreatedAt = DateTime.UtcNow},
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Tailor", Description = "Tailor role" , CreatedAt = DateTime.UtcNow},
                        new TafsilkPlatform.Models.Models.Role { Id = Guid.NewGuid(), Name = "Customer", Description = "Customer role" , CreatedAt = DateTime.UtcNow}
                    );
                    await db.SaveChangesAsync();
                    Log.Information("✓ Seeded default roles into database");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Applying migrations failed. Please ensure the database is available and migrations are compatible.");
                throw;
            }
        }
    }
    catch (Exception ex)
    {
        if (app.Environment.IsDevelopment())
        {
            Log.Warning(ex, "Database initialization failed during startup but continuing because environment is Development");
        }
        else
        {
            Log.Fatal(ex, "❌ Cannot initialize database. Application will stop.");
            throw;
        }
    }

    // ✅ STARTUP HEALTH CHECKS - fail fast if attachments folder not writable
    try
    {
        using var healthScope = app.Services.CreateScope();
        var healthService = healthScope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
        var healthReport = await healthService.CheckHealthAsync(h => h.Tags.Contains("attachments"));
        if (healthReport.Status != Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
        {
            var logger = healthScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogCritical("Startup aborted: attachments health check failed: {Status}. Details: {Entries}",
                healthReport.Status,
                string.Join("; ", healthReport.Entries.Select(e => $"{e.Key}={e.Value.Status}:{e.Value.Description}")));

            // Throw to abort startup and make failure explicit
            throw new InvalidOperationException("Attachments health check failed. See logs for details.");
        }
        else
        {
            Log.Information("✓ Attachments health check passed (writable + sufficient disk space)");
        }
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "❌ Startup health check for attachments failed and prevented application start");
        throw;
    }

    // ✅ PREVENT AUTOMATIC SHUTDOWN - Keep application running until explicitly stopped
    Log.Information("=== Application is now running ===");
    Log.Information("Press Ctrl+C to shut down");

    // Ensure console applications don't exit automatically
    if (Environment.UserInteractive)
    {
        Log.Information("Running in interactive mode - Application will stay open until manually closed");
    }

    app.Run();

    Log.Information("=== Application shutdown completed ===");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");

    // ✅ PREVENT AUTOMATIC CLOSE ON ERROR - Show error and wait for user input
    if (Environment.UserInteractive)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("❌ APPLICATION ERROR - The application encountered a fatal error");
        Console.WriteLine(new string('=', 80));
        Console.ResetColor();
        Console.WriteLine($"\nError: {ex.Message}");
        Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Press ANY KEY to exit...");
        Console.WriteLine(new string('=', 80));
        Console.ResetColor();
        Console.ReadKey();
    }

    throw;
}
finally
{
    Log.Information("=== Application cleanup in progress ===");
    Log.CloseAndFlush();

    // ✅ FINAL SAFETY NET - Ensure console stays open if running interactively
    if (Environment.UserInteractive && !Console.IsOutputRedirected)
    {
        // Small delay to ensure logs are flushed
        Thread.Sleep(500);
    }
}
