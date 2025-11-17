# 🚀 Quick Start - Using Shared Library

## 5-Minute Guide to Using TafsilkPlatform.Shared

---

## 📦 Step 1: Import Namespaces

```csharp
using TafsilkPlatform.Shared.Models;
using TafsilkPlatform.Shared.Constants;
using TafsilkPlatform.Shared.Utilities;
using TafsilkPlatform.Shared.Extensions;
```

---

## 🔐 Step 2: Password Hashing

### Hash a Password
```csharp
using TafsilkPlatform.Shared.Utilities;

string password = "123456";
string hash = PasswordHasher.HashPassword(password);
// Save hash to database
```

### Verify Password
```csharp
string inputPassword = "123456";
string storedHash = /* from database */;

bool isValid = PasswordHasher.VerifyPassword(inputPassword, storedHash);
if (isValid)
{
    // Password correct
}
```

---

## ✅ Step 3: Validation

### Egyptian Phone Number
```csharp
using TafsilkPlatform.Shared.Utilities;

string phone = "01012345678";
bool isValid = ValidationHelper.IsValidEgyptianPhone(phone);

if (!isValid)
{
    return "رقم هاتف غير صحيح";
}
```

### Email
```csharp
string email = "user@example.com";
bool isValid = ValidationHelper.IsValidEmail(email);

if (!isValid)
{
    return "البريد الإلكتروني غير صحيح";
}
```

### Sanitize Input
```csharp
string userInput = "  some text  ";
string clean = ValidationHelper.SanitizeInput(userInput);
// Result: "some text"
```

---

## 📅 Step 4: Date/Time Operations

### Current Time
```csharp
using TafsilkPlatform.Shared.Utilities;

DateTime utcNow = DateTimeHelper.UtcNow;
DateTime egyptNow = DateTimeHelper.EgyptNow;
```

### Format Arabic Date
```csharp
DateTime date = DateTime.Now;
string arabicDate = DateTimeHelper.FormatDateArabic(date);
// Result: "15/01/2025"
```

### Friendly Time Ago
```csharp
using TafsilkPlatform.Shared.Extensions;

DateTime orderDate = DateTime.Now.AddDays(-2);
string friendly = orderDate.ToFriendlyString();
// Result: "منذ 2 يوم"
```

---

## 💰 Step 5: Currency Formatting

```csharp
using TafsilkPlatform.Shared.Extensions;

decimal price = 1200.50m;

string formatted = price.ToEgyptianCurrency();
// Result: "1,201 جنيه"

string detailed = price.ToEgyptianCurrencyDetailed();
// Result: "1,200.50 جنيه"
```

---

## 📋 Step 6: Using Constants

### Roles
```csharp
using TafsilkPlatform.Shared.Constants;

if (user.Role == AppConstants.Roles.Customer)
{
  // Customer logic
}
else if (user.Role == AppConstants.Roles.Tailor)
{
    // Tailor logic
}
else if (user.Role == AppConstants.Roles.Admin)
{
    // Admin logic
}
```

### Order Status
```csharp
order.Status = AppConstants.OrderStatus.New;
order.Status = AppConstants.OrderStatus.InProgress;
order.Status = AppConstants.OrderStatus.Completed;
order.Status = AppConstants.OrderStatus.Cancelled;
```

### Error Messages
```csharp
if (profile == null)
{
    return (false, AppConstants.ErrorMessages.ProfileNotFound);
}

if (!authorized)
{
    return (false, AppConstants.ErrorMessages.Unauthorized);
}
```

### Cities
```csharp
var cities = AppConstants.Cities.Egyptian;
// List of Egyptian cities
```

---

## 📝 Step 7: String Extensions

```csharp
using TafsilkPlatform.Shared.Extensions;

// Check if empty
string name = "";
bool isEmpty = name.IsNullOrEmpty(); // true

// Truncate
string longText = "This is a very long text...";
string short = longText.Truncate(20);
// Result: "This is a very long..."

// Title case
string text = "محمد أحمد";
string title = text.ToTitleCase();
```

---

## 📊 Step 8: List Extensions

```csharp
using TafsilkPlatform.Shared.Extensions;

List<string> tailors = GetTailors();

// Check if empty
bool isEmpty = tailors.IsNullOrEmpty();

// Get random item
var randomTailor = tailors.GetRandom();

// Paginate
var page1 = tailors.Paginate(1, 10); // Page 1, 10 items per page
var page2 = tailors.Paginate(2, 10); // Page 2
```

---

## 🏗️ Step 9: Using DTOs

```csharp
using TafsilkPlatform.Shared.Models;

// Create tailor profile
var tailor = new TailorProfileDto
{
    Id = Guid.NewGuid(),
    UserId = Guid.NewGuid(),
    ShopName = "ورشة الأناقة",
    FullName = "محمد الخياط",
    Bio = "خياط محترف",
    City = "القاهرة",
    Address = "شارع فيصل",
    ExperienceYears = 15,
    Rating = 4.8m,
    ReviewCount = 124,
    Specialties = new List<string> { "بدلات رجالية", "فساتين سهرة" }
};

// Create order
var order = new OrderDto
{
    Id = Guid.NewGuid(),
    CustomerId = Guid.NewGuid(),
    CustomerName = "أحمد محمد",
    TailorId = tailor.Id,
    TailorName = tailor.ShopName,
    ServiceName = "تفصيل بدلة",
  TotalPrice = 1200m,
    Status = AppConstants.OrderStatus.New,
    OrderDate = DateTime.Now
};
```

---

## 🔄 Step 10: Complete Example

### Authentication Service

```csharp
using TafsilkPlatform.Shared.Utilities;
using TafsilkPlatform.Shared.Constants;
using TafsilkPlatform.Shared.Extensions;

public class AuthService
{
    public async Task<(bool Success, string? Error)> RegisterAsync(
string email, 
        string password, 
     string fullName,
        string phone)
    {
        // Validate email
        if (!ValidationHelper.IsValidEmail(email))
        {
   return (false, "البريد الإلكتروني غير صحيح");
        }

 // Validate phone
        if (!ValidationHelper.IsValidEgyptianPhone(phone))
{
     return (false, "رقم الهاتف غير صحيح");
     }

        // Sanitize name
  fullName = ValidationHelper.SanitizeInput(fullName);

        // Hash password
        string passwordHash = PasswordHasher.HashPassword(password);

        // Create user
        var user = new User
      {
        Id = IdGenerator.NewGuid(),
       Email = email,
       PasswordHash = passwordHash,
            FullName = fullName,
   PhoneNumber = phone,
       Role = AppConstants.Roles.Customer,
     CreatedAt = DateTimeHelper.UtcNow
        };

     // Save to database...

        return (true, null);
}

    public async Task<bool> LoginAsync(string email, string password)
    {
        // Get user from database
        var user = await GetUserByEmailAsync(email);
        if (user == null)
    {
        return false;
        }

   // Verify password
  return PasswordHasher.VerifyPassword(password, user.PasswordHash);
    }
}
```

### Profile Service with Shared Library

```csharp
using TafsilkPlatform.Shared.Utilities;
using TafsilkPlatform.Shared.Constants;

public class ProfileService
{
    public async Task<(bool Success, string? Error)> UpdateProfileAsync(
     Guid userId, 
      string fullName, 
    string city)
    {
        try
        {
  var profile = await GetProfileAsync(userId);
     if (profile == null)
 {
    return (false, AppConstants.ErrorMessages.ProfileNotFound);
    }

  // Sanitize input
    profile.FullName = ValidationHelper.SanitizeInput(fullName);
     profile.City = city;
      profile.UpdatedAt = DateTimeHelper.UtcNow;

            await SaveChangesAsync();
       
            return (true, null);
        }
   catch (Exception ex)
 {
      _logger.LogError(ex, "Error updating profile");
  return (false, AppConstants.ErrorMessages.GeneralError);
        }
    }
}
```

### Controller with Extensions

```csharp
using TafsilkPlatform.Shared.Extensions;
using TafsilkPlatform.Shared.Constants;
using TafsilkPlatform.Shared.Models;

public class OrdersController : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        var allOrders = await _dataService.GetAllOrdersAsync();
        
        // Paginate
 var orders = allOrders.Paginate(page, 10);

  // Create view model with formatted data
        var viewModel = orders.Select(o => new OrderViewModel
        {
   Id = o.Id,
            CustomerName = o.CustomerName,
        ServiceName = o.ServiceName,
            Price = o.TotalPrice.ToEgyptianCurrency(),
      Status = o.Status,
            OrderDate = o.OrderDate.ToFriendlyString()
    }).ToList();

    return View(viewModel);
    }
}
```

---

## ✅ Checklist

Before using the shared library, make sure:

- [ ] Added project reference: `dotnet add reference TafsilkPlatform.Shared`
- [ ] Imported necessary namespaces
- [ ] Replaced magic strings with AppConstants
- [ ] Using shared utilities for validation
- [ ] Using PasswordHasher for passwords
- [ ] Using extension methods for formatting
- [ ] Using DTOs for data transfer

---

## 🎯 Common Patterns

### Pattern 1: Validation Before Save
```csharp
// Validate
if (!ValidationHelper.IsValidEmail(email))
    return error;

if (!ValidationHelper.IsValidEgyptianPhone(phone))
    return error;

// Sanitize
email = ValidationHelper.SanitizeInput(email);

// Save...
```

### Pattern 2: Error Handling
```csharp
try
{
    // Operation
    await SaveAsync();
    return (true, null);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    return (false, AppConstants.ErrorMessages.GeneralError);
}
```

### Pattern 3: Display Formatting
```csharp
// In View/Controller
var viewModel = new
{
    Price = order.Price.ToEgyptianCurrency(),
    Date = order.Date.ToFriendlyString(),
    Status = order.Status // Already in Arabic from AppConstants
};
```

---

## 📚 More Resources

- **Complete Guide:** See `INTEGRATION_GUIDE.md`
- **Full Summary:** See `INTEGRATION_COMPLETE.md`

---

**Ready to use the shared library!** 🚀

*All code is consistent across Web and MVC projects!*
