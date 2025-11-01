# Tafsilk Platform - Quick Reference Guide

Quick reference for common commands, patterns, and configurations.

## Table of Contents
- [Common Commands](#common-commands)
- [Configuration](#configuration)
- [Code Patterns](#code-patterns)
- [Database](#database)
- [Testing](#testing)
- [Deployment](#deployment)

## Common Commands

### .NET CLI

```bash
# Build the project
dotnet build

# Run the project
dotnet run --project TafsilkPlatform.Web

# Watch mode (auto-reload on changes)
dotnet watch run --project TafsilkPlatform.Web

# Run tests
dotnet test

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean

# Publish for production
dotnet publish -c Release -o ./publish

# Check for outdated packages
dotnet list package --outdated

# Update a package
dotnet add package PackageName --version X.X.X
```

### Entity Framework Core

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project TafsilkPlatform.Web

# Update database to latest migration
dotnet ef database update --project TafsilkPlatform.Web

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project TafsilkPlatform.Web

# Remove last migration (if not applied to database)
dotnet ef migrations remove --project TafsilkPlatform.Web

# Generate SQL script from migrations
dotnet ef migrations script --project TafsilkPlatform.Web -o migration.sql

# Generate script for specific migration range
dotnet ef migrations script FromMigration ToMigration --project TafsilkPlatform.Web

# Drop database
dotnet ef database drop --project TafsilkPlatform.Web

# Get database connection info
dotnet ef dbcontext info --project TafsilkPlatform.Web

# List all migrations
dotnet ef migrations list --project TafsilkPlatform.Web
```

### User Secrets

```bash
# Initialize user secrets
dotnet user-secrets init --project TafsilkPlatform.Web

# Set a secret
dotnet user-secrets set "Key:SubKey" "Value" --project TafsilkPlatform.Web

# List all secrets
dotnet user-secrets list --project TafsilkPlatform.Web

# Remove a secret
dotnet user-secrets remove "Key:SubKey" --project TafsilkPlatform.Web

# Clear all secrets
dotnet user-secrets clear --project TafsilkPlatform.Web
```

### Git Commands

```bash
# Create feature branch
git checkout -b feature/my-feature

# Stage changes
git add .

# Commit changes
git commit -m "feat: add new feature"

# Push to remote
git push origin feature/my-feature

# Pull latest changes
git pull origin main

# Update your branch with main
git checkout main
git pull origin main
git checkout feature/my-feature
git merge main

# Squash commits
git rebase -i HEAD~3

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Undo last commit (discard changes)
git reset --hard HEAD~1

# View commit history
git log --oneline --graph
```

## Configuration

### Required User Secrets (Development)

```bash
# JWT Configuration
dotnet user-secrets set "Jwt:Key" "your-secret-key-here-minimum-32-chars"

# Google OAuth
dotnet user-secrets set "Google:client_id" "your-client-id.apps.googleusercontent.com"
dotnet user-secrets set "Google:client_secret" "your-client-secret"

# Facebook OAuth
dotnet user-secrets set "Facebook:app_id" "your-app-id"
dotnet user-secrets set "Facebook:app_secret" "your-app-secret"

# Email Configuration (Optional)
dotnet user-secrets set "Email:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
dotnet user-secrets set "Email:FromEmail" "noreply@tafsilk.com"
```

### Environment Variables (Production)

```bash
# Set environment variables (Linux/Mac)
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"
export Jwt__Key="your-production-jwt-key"
export Google__client_id="your-client-id"
export Google__client_secret="your-client-secret"

# Set environment variables (Windows PowerShell)
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__DefaultConnection="Server=...;Database=...;"
$env:Jwt__Key="your-production-jwt-key"
```

### Connection Strings

```json
// Development (LocalDB)
"Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

// Production (SQL Server)
"Server=your-server.database.windows.net;Database=TafsilkPlatformDb;User Id=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False;"

// Production (Azure SQL)
"Server=tcp:your-server.database.windows.net,1433;Initial Catalog=TafsilkPlatformDb;Persist Security Info=False;User ID=your-user;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## Code Patterns

### Repository Pattern

```csharp
// Interface
public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
    Task<Order?> GetOrderWithItemsAsync(Guid orderId);
}

// Implementation
public class OrderRepository : EfRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<List<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
  return await _context.Orders
 .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
    {
        return await _context.Orders
  .Include(o => o.OrderItems)
        .Include(o => o.Customer)
   .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
```

### Service Pattern

```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<OrderService> logger)
    {
     _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
 _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        try
  {
     var order = new Order
       {
  Id = Guid.NewGuid(),
       CustomerId = dto.CustomerId,
  TailorId = dto.TailorId,
   Status = OrderStatus.Pending,
      CreatedAt = DateTime.UtcNow
        };

  await _orderRepository.AddAsync(order);
       await _unitOfWork.SaveChangesAsync();

         _logger.LogInformation("Order {OrderId} created successfully", order.Id);
 return order;
        }
  catch (Exception ex)
  {
            _logger.LogError(ex, "Error creating order");
  throw;
   }
 }
}
```

### Controller Pattern

```csharp
[Authorize(Policy = "CustomerPolicy")]
public class OrdersController : Controller
{
 private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
   IOrderService orderService,
        ILogger<OrdersController> logger)
    {
   _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customerId = User.GetUserId();
        var orders = await _orderService.GetCustomerOrdersAsync(customerId);
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var order = await _orderService.CreateOrderAsync(model.ToDto());
     return RedirectToAction(nameof(Details), new { id = order.Id });
}
        catch (Exception ex)
  {
  _logger.LogError(ex, "Error creating order");
       ModelState.AddModelError("", "An error occurred while creating the order");
      return View(model);
      }
    }
}
```

### API Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersApiController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetOrders()
    {
        var customerId = User.GetUserId();
   var orders = await _orderService.GetCustomerOrdersAsync(customerId);
        return Ok(orders);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto dto)
    {
        var order = await _orderService.CreateOrderAsync(dto);
     return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}
```

## Database

### Common EF Core Queries

```csharp
// Get all
var all = await _context.Orders.ToListAsync();

// Get by ID
var order = await _context.Orders.FindAsync(id);

// Get with related data
var order = await _context.Orders
    .Include(o => o.OrderItems)
    .Include(o => o.Customer)
 .FirstOrDefaultAsync(o => o.Id == id);

// Get with filtering
var orders = await _context.Orders
    .Where(o => o.Status == OrderStatus.Pending)
    .ToListAsync();

// Get with pagination
var orders = await _context.Orders
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
.ToListAsync();

// Count
var count = await _context.Orders.CountAsync();

// Any/Exists
var exists = await _context.Orders.AnyAsync(o => o.Id == id);

// Read-only queries (no tracking)
var orders = await _context.Orders
    .AsNoTracking()
    .ToListAsync();

// Complex queries
var result = await _context.Orders
    .Where(o => o.Status == OrderStatus.Completed)
    .Include(o => o.OrderItems)
    .Select(o => new OrderSummary
    {
        Id = o.Id,
        CustomerName = o.Customer.Name,
        TotalItems = o.OrderItems.Count,
  TotalAmount = o.OrderItems.Sum(i => i.Price * i.Quantity)
    })
    .ToListAsync();
```

### Transaction Management

```csharp
// Using Unit of Work
await _unitOfWork.BeginTransactionAsync();
try
{
    await _orderRepository.AddAsync(order);
    await _paymentRepository.AddAsync(payment);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
  throw;
}

// Using DbContext directly
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    _context.Orders.Add(order);
    _context.Payments.Add(payment);
  await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Testing

### Unit Test Example (xUnit)

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _service;

  public OrderServiceTests()
    {
     _mockRepository = new Mock<IOrderRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<OrderService>>();
     _service = new OrderService(
       _mockRepository.Object,
_mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidDto_ReturnsOrder()
    {
      // Arrange
  var dto = new CreateOrderDto
        {
     CustomerId = Guid.NewGuid(),
      TailorId = Guid.NewGuid()
   };

        // Act
 var result = await _service.CreateOrderAsync(dto);

     // Assert
        Assert.NotNull(result);
   Assert.Equal(dto.CustomerId, result.CustomerId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
_mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

## Deployment

### Publish Commands

```bash
# Publish for Windows
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish/win

# Publish for Linux
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish/linux

# Publish self-contained (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/win-sc

# Publish with single file
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -o ./publish/single
```

### IIS Configuration

```xml
<!-- web.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet"
     arguments=".\TafsilkPlatform.Web.dll"
       stdoutLogEnabled="true"
     stdoutLogFile=".\logs\stdout"
        hostingModel="inprocess" />
</system.webServer>
</configuration>
```

### Docker (Future)

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TafsilkPlatform.Web/TafsilkPlatform.Web.csproj", "TafsilkPlatform.Web/"]
RUN dotnet restore "TafsilkPlatform.Web/TafsilkPlatform.Web.csproj"
COPY . .
WORKDIR "/src/TafsilkPlatform.Web"
RUN dotnet build "TafsilkPlatform.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TafsilkPlatform.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TafsilkPlatform.Web.dll"]
```

## Useful URLs

### Development
- Application: `https://localhost:7001`
- Swagger: `https://localhost:7001/swagger`
- Health Check: `https://localhost:7001/health`

### Production
- Application: `https://tafsilk.com`
- API: `https://api.tafsilk.com`
- Admin: `https://admin.tafsilk.com`

## Default Credentials

### Admin Account
```
Email: admin@tafsilk.com
Password: Admin@123
```

⚠️ **Change immediately after first login!**

## Support & Resources

- **Documentation**: See README.md, DEVELOPER_GUIDE.md
- **Issues**: https://github.com/AhmedTyson/Tafsilk/issues
- **Email**: support@tafsilk.com

---

**Last Updated**: January 2025
**Version**: 1.0.0
