# 🔗 Project Integration Guide

## Overview

This document explains how **TafsilkPlatform.Web** (Razor Pages) and **TafsilkPlatform.MVC** are integrated using the shared library **TafsilkPlatform.Shared**.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│             TafsilkPlatform Solution   │
├─────────────────────────────────────────────────────────┤
│   │
│  ┌──────────────────┐   ┌──────────────────┐  │
│  │ TafsilkPlatform  │   │ TafsilkPlatform  │      │
│  │     .Web   │   │.MVC        │           │
│  │ (Razor Pages)    │   │   (MVC)          │    │
│  └────────┬─────────┘   └────────┬─────────┘      │
│    │        │  │
│           └──────────┬───────────┘             │
│ │       │
│      ┌─────────▼─────────┐      │
│        │ TafsilkPlatform   │      │
│     │   .Shared         │      │
│ │ (Class Library)   │        │
│       │       │     │
└─────────────────────────────────────────────────────────┘
```

---

## 📦 Shared Library Structure

### TafsilkPlatform.Shared

```
TafsilkPlatform.Shared/
│
├── Models/
│   ├── UserProfile.cs              # Shared user models
│   ├── TailorProfileDto.cs         # Tailor data transfer object
│   ├── CustomerProfileDto.cs# Customer data transfer object
│   ├── ServiceDto.cs        # Service data transfer object
│   ├── OrderDto.cs                 # Order data transfer object
│   └── AddressDto.cs       # Address data transfer object
│
├── ViewModels/
│   └── AuthViewModels.cs   # Shared authentication view models
│
├── Interfaces/
│   └── ISharedServices.cs          # Shared service interfaces
│
├── Services/
│   └── IDataService.cs     # Shared data service interface
│
├── Constants/
│   └── AppConstants.cs        # Shared constants (Roles, Status, Cities, etc.)
│
├── Utilities/
│   └── SharedUtilities.cs      # Password hashing, validation, date helpers
│
└── Extensions/
    └── SharedExtensions.cs         # Extension methods for common operations
```

---

## 🔌 Integration Points

### 1. Shared Models

Both projects can use the same DTOs (Data Transfer Objects):

```csharp
using TafsilkPlatform.Shared.Models;

// In both Web and MVC projects
var tailor = new TailorProfileDto
{
    Id = Guid.NewGuid(),
    ShopName = "ورشة الأناقة",
    City = "القاهرة"
};
```

### 2. Shared Constants

Use consistent constants across projects:

```csharp
using TafsilkPlatform.Shared.Constants;

// Roles
if (user.Role == AppConstants.Roles.Customer)
{
    // Customer logic
}

// Order Status
order.Status = AppConstants.OrderStatus.Completed;

// Error Messages
return (false, AppConstants.ErrorMessages.ProfileNotFound);
```

### 3. Shared Utilities

Common utility functions:

```csharp
using TafsilkPlatform.Shared.Utilities;

// Password hashing (same in both projects)
var hashedPassword = PasswordHasher.HashPassword("123456");
bool isValid = PasswordHasher.VerifyPassword("123456", hashedPassword);

// Validation
bool isValidPhone = ValidationHelper.IsValidEgyptianPhone("01012345678");
bool isValidEmail = ValidationHelper.IsValidEmail("test@example.com");

// Date formatting
string arabicDate = DateTimeHelper.FormatDateArabic(DateTime.Now);
DateTime egyptTime = DateTimeHelper.EgyptNow;
```

### 4. Shared Extensions

Use extension methods:

```csharp
using TafsilkPlatform.Shared.Extensions;

// String extensions
string name = "  محمد  ";
name = name.SanitizeInput(); // "محمد"

string longText = "هذا نص طويل جداً...";
string short = longText.Truncate(20); // "هذا نص طويل جداً..."

// Decimal extensions
decimal price = 1200.50m;
string formatted = price.ToEgyptianCurrency(); // "1,201 جنيه"

// DateTime extensions
DateTime orderDate = DateTime.Now.AddDays(-2);
string friendly = orderDate.ToFriendlyString(); // "منذ 2 يوم"

// List extensions
var tailors = GetTailors();
var paginated = tailors.Paginate(1, 10); // First page, 10 items
```

---

## 🔄 Data Service Integration

### IDataService Interface

Both projects implement the same interface:

```csharp
public interface IDataService
{
    Task<List<TailorProfileDto>> GetAllTailorsAsync();
    Task<TailorProfileDto?> GetTailorByIdAsync(Guid id);
    Task<List<ServiceDto>> GetServicesByTailorIdAsync(Guid tailorId);
    Task<List<OrderDto>> GetAllOrdersAsync();
    // ... more methods
}
```

### MVC Implementation (Mock Data)

```csharp
// In MVC: SharedDataAdapter.cs
public class SharedDataAdapter : BaseDataService
{
private readonly MockDataService _mockDataService;
    
    public override async Task<List<TailorProfileDto>> GetAllTailorsAsync()
    {
        return _mockDataService.GetAllTailors()
            .Select(t => new TailorProfileDto { /* map properties */ })
            .ToList();
    }
}
```

### Web Implementation (Real Database)

```csharp
// In Web: Can create DatabaseDataService.cs
public class DatabaseDataService : BaseDataService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public override async Task<List<TailorProfileDto>> GetAllTailorsAsync()
    {
        var tailors = await _unitOfWork.Tailors.GetAllAsync();
        return tailors.Select(t => new TailorProfileDto { /* map */ }).ToList();
 }
}
```

---

## 📝 Usage Examples

### Example 1: Authentication in Both Projects

**MVC (using shared utilities):**
```csharp
using TafsilkPlatform.Shared.Utilities;
using TafsilkPlatform.Shared.Constants;

public class AuthService
{
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = _users.FirstOrDefault(u => u.Email == email);
        if (user == null) return null;

    // Use shared password hasher
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
          return null;

        return user;
    }
}
```

**Web (using same utilities):**
```csharp
using TafsilkPlatform.Shared.Utilities;

public class AuthenticationService
{
    public async Task<bool> ValidatePasswordAsync(string password, string hash)
    {
 // Same password verification logic
   return PasswordHasher.VerifyPassword(password, hash);
    }
}
```

### Example 2: Using Shared Constants

**In both projects:**
```csharp
using TafsilkPlatform.Shared.Constants;

// Consistent role checking
if (User.IsInRole(AppConstants.Roles.Admin))
{
    // Admin logic
}

// Consistent status
order.Status = AppConstants.OrderStatus.InProgress;

// Consistent error messages
if (profile == null)
    return (false, AppConstants.ErrorMessages.ProfileNotFound);

// Consistent cities
var cities = AppConstants.Cities.Egyptian;
```

### Example 3: Data Transfer Between Projects

**Shared DTO:**
```csharp
using TafsilkPlatform.Shared.Models;

// Both projects can serialize/deserialize this
var tailor = new TailorProfileDto
{
    Id = Guid.NewGuid(),
    ShopName = "ورشة الأناقة",
    FullName = "محمد الخياط",
    City = "القاهرة",
    Rating = 4.8m
};

// Can be used in API, JSON, or direct transfer
```

---

## 🚀 Benefits of Integration

### 1. Code Reusability
- ✅ Write once, use everywhere
- ✅ No duplicate models or utilities
- ✅ Consistent business logic

### 2. Maintainability
- ✅ Update in one place
- ✅ Consistent behavior
- ✅ Easier refactoring

### 3. Type Safety
- ✅ Shared interfaces
- ✅ Compile-time checking
- ✅ No magic strings

### 4. Consistency
- ✅ Same validation rules
- ✅ Same error messages
- ✅ Same constants

### 5. Future API Integration
- ✅ Ready for web API
- ✅ Easy to add microservices
- ✅ Clear contracts

---

## 🔧 Setup Instructions

### 1. Add Project References

Already done:
```bash
# Web project references Shared
dotnet add TafsilkPlatform.Web reference TafsilkPlatform.Shared

# MVC project references Shared
dotnet add TafsilkPlatform.MVC reference TafsilkPlatform.Shared
```

### 2. Register Services (MVC)

In `TafsilkPlatform.MVC/Program.cs`:
```csharp
using TafsilkPlatform.Shared.Services;

builder.Services.AddScoped<IDataService, SharedDataAdapter>();
```

### 3. Register Services (Web)

In `TafsilkPlatform.Web/Program.cs`:
```csharp
using TafsilkPlatform.Shared.Services;

// When you create DatabaseDataService
builder.Services.AddScoped<IDataService, DatabaseDataService>();
```

### 4. Use in Controllers

**MVC:**
```csharp
public class TailorsController : Controller
{
    private readonly IDataService _dataService;

    public TailorsController(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
   var tailors = await _dataService.GetAllTailorsAsync();
 return View(tailors);
 }
}
```

**Web:**
```csharp
public class TailorsPageModel : PageModel
{
    private readonly IDataService _dataService;

    public TailorsPageModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task OnGetAsync()
    {
        Tailors = await _dataService.GetAllTailorsAsync();
    }
}
```

---

## 📊 Migration Strategy

### Phase 1: Current State ✅
- ✅ Shared library created
- ✅ Common models defined
- ✅ Utilities implemented
- ✅ MVC integrated with shared

### Phase 2: Web Integration (Next Step)
- [ ] Create `DatabaseDataService` in Web project
- [ ] Implement `IDataService` with EF Core
- [ ] Update controllers/pages to use shared DTOs
- [ ] Migrate utilities to use shared library

### Phase 3: API Layer (Future)
- [ ] Create API project
- [ ] Use shared DTOs for contracts
- [ ] Both Web and MVC consume API
- [ ] Microservices architecture

---

## 🎯 Quick Reference

### Import Shared Namespace

```csharp
// Models
using TafsilkPlatform.Shared.Models;

// Constants
using TafsilkPlatform.Shared.Constants;

// Utilities
using TafsilkPlatform.Shared.Utilities;

// Extensions
using TafsilkPlatform.Shared.Extensions;

// Services
using TafsilkPlatform.Shared.Services;
```

### Common Patterns

**Password Hashing:**
```csharp
var hash = PasswordHasher.HashPassword(password);
var isValid = PasswordHasher.VerifyPassword(password, hash);
```

**Validation:**
```csharp
bool validPhone = ValidationHelper.IsValidEgyptianPhone(phone);
bool validEmail = ValidationHelper.IsValidEmail(email);
```

**Date Formatting:**
```csharp
string arabicDate = DateTimeHelper.FormatDateArabic(date);
string friendlyTime = date.ToFriendlyString();
```

**Currency Formatting:**
```csharp
string price = amount.ToEgyptianCurrency(); // "1,200 جنيه"
```

**Constants:**
```csharp
// Roles
AppConstants.Roles.Customer
AppConstants.Roles.Tailor
AppConstants.Roles.Admin

// Status
AppConstants.OrderStatus.Completed
AppConstants.OrderStatus.InProgress

// Cities
AppConstants.Cities.Egyptian

// Error Messages
AppConstants.ErrorMessages.ProfileNotFound
```

---

## 🔄 Synchronization

### Keeping Projects in Sync

1. **Models**: Use shared DTOs
2. **Constants**: Use AppConstants
3. **Utilities**: Use shared helpers
4. **Validation**: Use ValidationHelper
5. **Business Logic**: Implement IDataService

### When to Update Shared Library

Update when:
- ✅ Adding new DTOs
- ✅ Adding new constants
- ✅ Adding utility functions
- ✅ Changing shared interfaces

Both projects will get updates automatically!

---

## 🎉 Summary

You now have:
- ✅ Shared class library (TafsilkPlatform.Shared)
- ✅ Common models (DTOs)
- ✅ Shared constants
- ✅ Utility functions
- ✅ Extension methods
- ✅ Service interfaces
- ✅ MVC integrated with shared
- ✅ Ready for Web integration

**Next Step**: Integrate TafsilkPlatform.Web with the shared library!

---

**Benefits Achieved:**
- 🎯 Code reusability
- 🔒 Type safety
- 📏 Consistency
- 🚀 Future-ready for APIs
- 🧩 Easy maintenance

---

*Integration Complete!* 🎉
