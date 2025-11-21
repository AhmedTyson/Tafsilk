# 🔧 Easy Maintenance Guide

## 🎯 Philosophy: Keep It Simple

This project is designed to be **maintenance-free** and **easy to understand**. Everything is automated where possible.

---

## 📝 Common Maintenance Tasks

### 1. Add a New Feature (5 Steps)

```csharp
// Step 1: Create Model (Models/YourModel.cs)
public class YourModel 
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

// Step 2: Add to DbContext (Data/AppDbContext.cs)
public virtual DbSet<YourModel> YourModels { get; set; }

// Step 3: Create Repository (Repositories/YourRepository.cs)
public class YourRepository : EfRepository<YourModel>, IYourRepository
{
    // Add custom methods here
}

// Step 4: Create Service (Services/YourService.cs)
public class YourService : IYourService
{
    // Business logic here
}

// Step 5: Create Controller (Controllers/YourController.cs)
public class YourController : Controller
{
    // API endpoints here
}

// Step 6: Run migration
dotnet ef migrations add AddYourModel
dotnet ef database update
```

**That's it!** No complex setup needed.

---

## 🛠️ Helper Methods (Use These!)

### Configuration Helper
```csharp
// Get config with clear error
var value = config.GetRequiredValue("Key", "Friendly Name");

// Get config with default
var value = config.GetValueWithDefault("Key", "default", "Friendly Name");
```

### Validation Helper
```csharp
// Validate email
if (ValidationHelper.IsValidEmail(email)) { ... }

// Validate password
var (isValid, error) = ValidationHelper.ValidatePassword(password);
```

### Error Helper
```csharp
// Get user-friendly error message
var message = ErrorHelper.GetUserFriendlyMessage(exception);

// Log error simply
ErrorHelper.LogError(logger, ex, "Context", param1, param2);
```

---

## 🔍 Finding Things

### Where is...?

| What | Where |
|------|-------|
| Database Models | `Models/` |
| Business Logic | `Services/` |
| Data Access | `Repositories/` |
| API Endpoints | `Controllers/` |
| Configuration | `appsettings.json` |
| Database Setup | `Data/AppDbContext.cs` |
| Helpers | `Helpers/` |

---

## 🐛 Debugging Made Easy

### 1. Check Logs
```bash
# Logs are in Logs/ folder
cat Logs/log-*.txt  # Linux/Mac
type Logs\log-*.txt  # Windows
```

### 2. Check Health
Visit: `https://localhost:7186/health`

### 3. Check Swagger
Visit: `https://localhost:7186/swagger`

### 4. Common Issues

**Problem**: "JWT Key not configured"  
**Fix**: `dotnet user-secrets set "Jwt:Key" "YourKey"`

**Problem**: "Database error"  
**Fix**: `dotnet ef database update`

**Problem**: "Port in use"  
**Fix**: Change port in `Properties/launchSettings.json`

---

## 📋 Code Patterns (Copy & Paste)

### Simple Controller
```csharp
public class MyController : Controller
{
    private readonly IMyService _service;
    private readonly ILogger<MyController> _logger;

    public MyController(IMyService service, ILogger<MyController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var data = await _service.GetDataAsync();
            return View(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index");
            TempData["Error"] = ErrorHelper.GetUserFriendlyMessage(ex);
            return RedirectToAction("Index", "Home");
        }
    }
}
```

### Simple Service
```csharp
public class MyService : IMyService
{
    private readonly IMyRepository _repository;
    private readonly ILogger<MyService> _logger;

    public MyService(IMyRepository repository, ILogger<MyService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<MyModel> GetDataAsync()
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

---

## ✅ Best Practices (Already Built In)

1. ✅ **Error Handling**: Global exception handler catches everything
2. ✅ **Validation**: Use `ValidationHelper` for common validations
3. ✅ **Logging**: Automatic logging with Serilog
4. ✅ **Security**: Headers and authentication already configured
5. ✅ **Database**: Auto-migration on startup
6. ✅ **Configuration**: Clear error messages if missing

---

## 🚫 Don't Do This

❌ Don't hardcode secrets in code  
❌ Don't skip error handling  
❌ Don't forget to validate inputs  
❌ Don't commit secrets to git  
❌ Don't use `NoTracking` unless you need it

---

## 💡 Pro Tips

1. **Use Helpers**: Don't write validation/error handling from scratch
2. **Check Logs First**: Most issues are logged clearly
3. **Use Swagger**: Test APIs easily at `/swagger`
4. **Health Checks**: Monitor app health at `/health`
5. **Read Error Messages**: They tell you exactly what to do

---

## 📚 Quick Reference

```bash
# Setup
dotnet user-secrets set "Jwt:Key" "YourKey"

# Run
dotnet run

# Database
dotnet ef migrations add Name
dotnet ef database update

# Build
dotnet build

# Clean
dotnet clean
```

---

**Remember**: If something is hard, it's probably not needed. Keep it simple! 🎯

