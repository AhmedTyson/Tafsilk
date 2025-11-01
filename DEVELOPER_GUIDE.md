# Tafsilk Platform - Developer Guide

Complete guide for developers working on the Tafsilk Platform.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Getting Started](#getting-started)
4. [Project Structure](#project-structure)
5. [Database](#database)
6. [Authentication & Authorization](#authentication--authorization)
7. [API Endpoints](#api-endpoints)
8. [Common Tasks](#common-tasks)
9. [Troubleshooting](#troubleshooting)

## Project Overview

Tafsilk is a tailoring marketplace platform built with:
- **.NET 9** - Latest ASP.NET Core framework
- **Razor Pages** - Server-side rendering
- **Entity Framework Core** - ORM for database access
- **SQL Server** - Relational database
- **JWT & Cookie Auth** - Dual authentication strategy

### Key Features
- Multi-role user system (Customer, Tailor, Corporate, Admin)
- Order management and tracking
- RFQ (Request for Quote) system
- Tailor portfolio and verification
- Reviews and ratings
- Wallet and payment system
- OAuth integration (Google, Facebook)

## Architecture

### Architectural Pattern
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Dependency Injection** - Loose coupling
- **Service Layer** - Business logic separation

### Layers

```
┌─────────────────────────────────────┐
│         Presentation Layer     │
│    (Controllers, Views, ViewModels) │
└──────────────┬──────────────────────┘
   │
┌──────────────▼──────────────────────┐
│          Service Layer          │
│   (Business Logic, Validation) │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│        Repository Layer         │
│      (Data Access, EF Core)         │
└──────────────┬──────────────────────┘
   │
┌──────────────▼──────────────────────┐
│         Database Layer              │
│       (SQL Server)     │
└─────────────────────────────────────┘
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/AhmedTyson/Tafsilk.git
cd Tafsilk
```

### 2. Install Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) or LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

### 3. Configure Secrets

**Using dotnet CLI:**
```bash
cd TafsilkPlatform.Web

# JWT Secret (generate a secure random key)
dotnet user-secrets set "Jwt:Key" "YOUR_SECURE_SECRET_KEY_HERE"

# Google OAuth (get from Google Cloud Console)
dotnet user-secrets set "Google:client_id" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Google:client_secret" "YOUR_GOOGLE_CLIENT_SECRET"

# Facebook OAuth (get from Facebook Developers)
dotnet user-secrets set "Facebook:app_id" "YOUR_FACEBOOK_APP_ID"
dotnet user-secrets set "Facebook:app_secret" "YOUR_FACEBOOK_APP_SECRET"

# Email Configuration (optional, for sending emails)
dotnet user-secrets set "Email:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
```

**Using Visual Studio:**
1. Right-click on `TafsilkPlatform.Web` project
2. Select "Manage User Secrets"
3. Add secrets in JSON format

### 4. Database Setup

```bash
# Navigate to the project directory
cd TafsilkPlatform.Web

# Apply migrations
dotnet ef database update

# Verify database is created
# Connect to (localdb)\MSSQLLocalDB
# Check for TafsilkPlatformDb_Dev database
```

### 5. Run the Application

```bash
dotnet run --project TafsilkPlatform.Web
```

Or press `F5` in Visual Studio.

**Application will be available at:**
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:7001/swagger` (Development only)
- Health Check: `https://localhost:7001/health`

### 6. Default Admin Login

```
Email: admin@tafsilk.com
Password: Admin@123
```

⚠️ **Change this password immediately!**

## Project Structure

```
TafsilkPlatform.Web/
├── Configuration/        # Configuration options and settings
├── Controllers/    # MVC Controllers
│   ├── AccountController.cs          # Authentication
│   ├── OrdersController.cs         # Order management
│   ├── ProfilesController.cs         # Profile management
│   ├── AdminDashboardController.cs   # Admin functions
│   └── Api*.cs    # API controllers
├── Data/  # Database context and migrations
│   ├── AppDbContext.cs          # EF Core DbContext
│   ├── UnitOfWork.cs          # Unit of Work pattern
│   ├── Migrations/ # EF migrations
│   └── Seed/       # Data seeding
├── Extensions/   # Extension methods
│   └── ClaimsPrincipalExtensions.cs
├── Interfaces/             # Service and repository interfaces
│   ├── IRepository.cs
│   ├── IUserRepository.cs
│   ├── IOrderRepository.cs
│   └── ...
├── Middleware/  # Custom middleware
│   ├── UserStatusMiddleware.cs
│   └── GlobalExceptionHandlerMiddleware.cs
├── Models/        # Domain models (entities)
│   ├── User.cs
│   ├── TailorProfile.cs
│   ├── Order.cs
│   ├── RFQ.cs
│   └── ...
├── Repositories/           # Data access implementations
│   ├── EfRepository.cs  # Generic repository
│   ├── UserRepository.cs
│   ├── OrderRepository.cs
│   └── ...
├── Security/               # Authentication & authorization
│├── PasswordHasher.cs
│   ├── TokenService.cs
│   ├── AuthorizationAttributes.cs
│   └── AuthorizationHandlers.cs
├── Services/               # Business logic services
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── ProfileService.cs
│   ├── EmailService.cs
│└── ...
├── ViewModels/     # DTOs for views
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   └── ...
├── Views/        # Razor views
│   ├── Account/
│   ├── Orders/
│   ├── Profiles/
│   ├── Shared/
│   └── ...
├── wwwroot/         # Static files
│   ├── css/
│   ├── js/
│   ├── images/
│   └── uploads/
├── appsettings.json # Configuration (non-sensitive)
├── appsettings.Development.json
├── Program.cs    # Application entry point
└── TafsilkPlatform.Web.csproj
```

## Database

### Entity Relationship Overview

```
User 1─────* TailorProfile
     1─────* CustomerProfile
     1─────* CorporateAccount
     1─────* UserAddress
     1─────* RefreshToken
     1─────* Wallet

TailorProfile 1────* TailorService
       1────* PortfolioImage
      1────* Review
      *────* Order

Order 1────* OrderItem
      1────* Payment
      1────* Review

RFQ 1────* RFQBid
```

### Key Tables

| Table | Description |
|-------|-------------|
| `Users` | Core user accounts |
| `Roles` | User roles (Admin, Tailor, Customer, Corporate) |
| `TailorProfiles` | Tailor-specific information |
| `CustomerProfiles` | Customer-specific information |
| `CorporateAccounts` | Corporate account information |
| `Orders` | Order management |
| `OrderItems` | Individual items in orders |
| `RFQs` | Request for Quote |
| `RFQBids` | Tailor bids on RFQs |
| `Reviews` | Customer reviews |
| `Payments` | Payment transactions |
| `Wallets` | User wallets |
| `Notifications` | User notifications |

### Creating Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName --project TafsilkPlatform.Web

# Update database
dotnet ef database update --project TafsilkPlatform.Web

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project TafsilkPlatform.Web

# Remove last migration (if not applied)
dotnet ef migrations remove --project TafsilkPlatform.Web

# Generate SQL script
dotnet ef migrations script --project TafsilkPlatform.Web --output migration.sql
```

## Authentication & Authorization

### Authentication Schemes

1. **Cookie Authentication** (Default for web)
   - Used for Razor Pages
   - 14-day sliding expiration

2. **JWT Bearer** (For API)
   - Used for API endpoints
   - 60-minute access token
   - 30-day refresh token

### Authorization Policies

| Policy | Description | Roles |
|--------|-------------|-------|
| `AdminPolicy` | Admin access | Admin |
| `TailorPolicy` | Tailor access | Tailor |
| `CustomerPolicy` | Customer access | Customer |
| `CorporatePolicy` | Corporate access | Corporate |
| `VerifiedTailorPolicy` | Verified tailors only | Tailor (verified) |
| `ApprovedCorporatePolicy` | Approved corporates | Corporate (approved) |
| `AuthenticatedPolicy` | Any authenticated user | Any |
| `CustomerOrTailorPolicy` | Customer or Tailor | Customer, Tailor |
| `ServiceProviderPolicy` | Service providers | Tailor, Corporate |

### Using Authorization in Controllers

```csharp
[Authorize(Policy = "TailorPolicy")]
public class TailorManagementController : Controller
{
    [Authorize(Policy = "VerifiedTailorPolicy")]
    public IActionResult ManageServices()
{
   // Only verified tailors can access
    }
}
```

### Using Authorization in Views

```cshtml
@if (User.IsInRole("Tailor"))
{
    <a href="/TailorManagement/Dashboard">My Dashboard</a>
}

@if (User.HasClaim("IsVerified", "True"))
{
    <span class="badge badge-success">Verified</span>
}
```

## API Endpoints

### Authentication APIs

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "phoneNumber": "+20123456789",
  "role": "Customer"
}
```

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
"password": "Password123!"
}
```

### Order APIs

```http
GET /api/orders
Authorization: Bearer {token}

Response:
[
  {
    "id": "guid",
    "customerName": "John Doe",
    "status": "Pending",
    "totalAmount": 500.00
  }
]
```

### Testing with Swagger

1. Run the application in Development mode
2. Navigate to `https://localhost:7001/swagger`
3. Click "Authorize" and enter your JWT token
4. Test endpoints interactively

## Common Tasks

### 1. Adding a New Entity

**Step 1: Create the model**
```csharp
// Models/NewEntity.cs
public class NewEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

**Step 2: Add DbSet to AppDbContext**
```csharp
// Data/AppDbContext.cs
public DbSet<NewEntity> NewEntities => Set<NewEntity>();
```

**Step 3: Create migration**
```bash
dotnet ef migrations add AddNewEntity --project TafsilkPlatform.Web
dotnet ef database update --project TafsilkPlatform.Web
```

### 2. Creating a New Repository

**Step 1: Create interface**
```csharp
// Interfaces/INewEntityRepository.cs
public interface INewEntityRepository : IRepository<NewEntity>
{
    Task<List<NewEntity>> GetActiveEntitiesAsync();
}
```

**Step 2: Implement repository**
```csharp
// Repositories/NewEntityRepository.cs
public class NewEntityRepository : EfRepository<NewEntity>, INewEntityRepository
{
    public NewEntityRepository(AppDbContext context) : base(context) { }

    public async Task<List<NewEntity>> GetActiveEntitiesAsync()
    {
        return await _context.NewEntities
        .Where(e => e.IsActive)
        .ToListAsync();
    }
}
```

**Step 3: Register in Program.cs**
```csharp
builder.Services.AddScoped<INewEntityRepository, NewEntityRepository>();
```

### 3. Adding a New Service

```csharp
// Services/NewEntityService.cs
public class NewEntityService : INewEntityService
{
    private readonly INewEntityRepository _repository;
    private readonly ILogger<NewEntityService> _logger;

    public NewEntityService(
        INewEntityRepository repository,
    ILogger<NewEntityService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<NewEntity> CreateAsync(CreateNewEntityDto dto)
    {
        var entity = new NewEntity
  {
  Id = Guid.NewGuid(),
    Name = dto.Name,
          CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity);
    await _repository.SaveChangesAsync();

        _logger.LogInformation("Created new entity {EntityId}", entity.Id);
        return entity;
    }
}
```

### 4. Adding a New Controller

```csharp
[Authorize(Policy = "AuthenticatedPolicy")]
public class NewEntityController : Controller
{
    private readonly INewEntityService _service;

    public NewEntityController(INewEntityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
  var entities = await _service.GetAllAsync();
      return View(entities);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNewEntityDto dto)
    {
if (!ModelState.IsValid)
            return View(dto);

        var entity = await _service.CreateAsync(dto);
return RedirectToAction(nameof(Index));
    }
}
```

## Troubleshooting

### Database Connection Issues

**Problem:** Cannot connect to LocalDB
```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error...
```

**Solution:**
```bash
# Check if LocalDB is running
sqllocaldb info

# Start LocalDB
sqllocaldb start MSSQLLocalDB

# Verify connection string in appsettings.json
```

### Migration Issues

**Problem:** Pending model changes
```
The model for the context has changed since the database was created.
```

**Solution:**
```bash
# Create a new migration
dotnet ef migrations add FixModelChanges --project TafsilkPlatform.Web

# Apply migration
dotnet ef database update --project TafsilkPlatform.Web
```

### OAuth Issues

**Problem:** OAuth callback fails

**Solution:**
1. Verify redirect URIs in OAuth provider settings
2. Check that secrets are properly configured
3. Ensure HTTPS is enabled (or use HTTP in development)

### Build Errors

**Problem:** Package restore fails

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild solution
dotnet build
```

## Performance Tips

1. **Use async/await** for all I/O operations
2. **Enable response compression** (already configured)
3. **Use pagination** for large datasets
4. **Implement caching** where appropriate
5. **Use `AsNoTracking()`** for read-only queries
6. **Index database columns** used in WHERE clauses

## Debugging

### Enable Detailed Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
  "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Use Breakpoints

- Set breakpoints in Visual Studio with `F9`
- Use conditional breakpoints for specific scenarios
- Inspect variables in the Watch window

### Database Profiling

Use SQL Server Profiler or EF Core logging to analyze queries:

```csharp
// Enable EF logging
optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
```

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp)
- [Tafsilk GitHub Repository](https://github.com/AhmedTyson/Tafsilk)

## Need Help?

- 📧 Email: support@tafsilk.com
- 💬 GitHub Discussions: [Ask a question](https://github.com/AhmedTyson/Tafsilk/discussions)
- 🐛 Bug Reports: [Create an issue](https://github.com/AhmedTyson/Tafsilk/issues)

---

Happy coding! 🚀
