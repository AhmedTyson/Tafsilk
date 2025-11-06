# FluentValidation Quick Reference - Tafsilk Platform

## 🚀 Quick Start Guide

### How to Use ValidationService in Your Controller

```csharp
public class ProfilesController : Controller
{
    private readonly IValidationService _validationService;
    private readonly IProfileService _profileService;

    public ProfilesController(IValidationService validationService, IProfileService profileService)
    {
        _validationService = validationService;
        _profileService = profileService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UpdateCustomerProfileRequest request)
    {
        // Step 1: Validate with FluentValidation
    var validationResult = await _validationService.ValidateCustomerProfileAsync(request);

  if (!validationResult.IsValid)
        {
            // Add errors to ModelState
       foreach (var error in validationResult.Errors)
            {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
      }
            return View(request); // Return form with errors
     }

        // Step 2: Process business logic
        var result = await _profileService.UpdateCustomerProfileAsync(userId, request);

  if (!result.Success)
        {
     ModelState.AddModelError("", result.ErrorMessage);
            return View(request);
        }

        // Step 3: Success
    TempData["SuccessMessage"] = "تم التحديث بنجاح!";
        return RedirectToAction("Profile");
    }
}
```

---

## 📋 Available Validators

| Validator | Method | Use Case |
|-----------|--------|----------|
| **CompleteTailorProfileValidator** | `ValidateCompleteTailorProfileAsync()` | Initial tailor registration |
| **CustomerProfileValidator** | `ValidateCustomerProfileAsync()` | Customer profile updates |
| **TailorProfileValidator** | `ValidateTailorProfileAsync()` | Tailor profile updates |
| **AddressValidator** | `ValidateAddressAsync()` | Add/edit delivery address |
| **ServiceValidator** | `ValidateServiceAsync()` | Add/edit tailor services |

---

## 🔧 Common Validation Patterns

### Pattern 1: Simple Required Field
```csharp
RuleFor(x => x.FullName)
    .NotEmpty().WithMessage("الاسم مطلوب");
```

### Pattern 2: Length Constraints
```csharp
RuleFor(x => x.FullName)
    .NotEmpty().WithMessage("الاسم مطلوب")
 .MinimumLength(3).WithMessage("الاسم يجب أن يكون 3 أحرف على الأقل")
    .MaximumLength(100).WithMessage("الاسم لا يمكن أن يتجاوز 100 حرف");
```

### Pattern 3: Regex Pattern Matching
```csharp
RuleFor(x => x.PhoneNumber)
    .NotEmpty().WithMessage("رقم الهاتف مطلوب")
 .Matches(@"^01[0-2,5]\d{8}$").WithMessage("رقم هاتف مصري غير صحيح");
```

### Pattern 4: Range Validation
```csharp
RuleFor(x => x.ExperienceYears)
    .GreaterThanOrEqualTo(0).WithMessage("سنوات الخبرة لا يمكن أن تكون سالبة")
    .LessThanOrEqualTo(60).WithMessage("سنوات الخبرة لا يمكن أن تتجاوز 60 عاماً");
```

### Pattern 5: Conditional Validation
```csharp
RuleFor(x => x.DateOfBirth)
    .LessThan(DateTime.Now.AddYears(-13))
    .When(x => x.DateOfBirth.HasValue)
    .WithMessage("يجب أن يكون العمر 13 عاماً على الأقل");
```

### Pattern 6: Custom Validation Logic
```csharp
RuleFor(x => x.Gender)
    .Must(x => x == "Male" || x == "Female" || x == "ذكر" || x == "أنثى")
    .WithMessage("يجب اختيار ذكر أو أنثى");
```

### Pattern 7: File Upload Validation
```csharp
RuleFor(x => x.IdDocument)
    .NotNull().WithMessage("يجب تحميل صورة الهوية")
    .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
    .WithMessage("حجم الملف يجب ألا يتجاوز 10 ميجابايت")
    .Must(IsValidImageFile)
    .WithMessage("نوع الملف غير مدعوم");
```

---

## 📱 Egyptian Phone Number Validation

**Regex Pattern:** `^01[0-2,5]\d{8}$`

**Valid Examples:**
- 01012345678 (Vodafone)
- 01112345678 (Etisalat)
- 01212345678 (Orange)
- 01512345678 (WE)

**Invalid Examples:**
- 0101234567 (too short)
- 010123456789 (too long)
- 01312345678 (invalid prefix)
- 1012345678 (missing leading 0)

---

## 🌍 Arabic Character Validation

**Pattern for Arabic/English Names:**
```csharp
.Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$")
```

**Unicode Range:**
- `\u0600-\u06FF` = Arabic characters
- `a-zA-Z` = English letters
- `\s` = Whitespace

---

## 📦 File Upload Validation Helper

```csharp
private bool IsValidImageFile(IFormFile? file)
{
    if (file == null) return false;

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    var allowedContentTypes = new[] {
        "image/jpeg", "image/jpg", "image/png", "image/webp",
        "application/pdf"
    };

    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    return allowedExtensions.Contains(extension) &&
    allowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
}
```

**Usage:**
```csharp
RuleFor(x => x.ProfilePicture)
    .Must(IsValidImageFile).When(x => x.ProfilePicture != null)
    .WithMessage("الصورة يجب أن تكون بصيغة JPG أو PNG");
```

---

## 🎯 Error Message Best Practices

### Good Error Messages (Arabic):
✅ "الاسم الكامل مطلوب"  
✅ "رقم هاتف مصري غير صحيح (مثال: 01012345678)"  
✅ "الوصف يجب أن يكون بين 10 و 500 حرف"  
✅ "يجب تحميل على الأقل 3 صور"  

### Bad Error Messages:
❌ "Invalid"  
❌ "Field required"  
❌ "Error"  

---

## 🧪 Testing Validation Rules

### Unit Test Example:

```csharp
[Test]
public async Task ValidatePhoneNumber_WithValidEgyptianNumber_Passes()
{
    // Arrange
    var validator = new CustomerProfileValidator();
    var request = new UpdateCustomerProfileRequest
  {
  FullName = "Ahmed Hassan",
        PhoneNumber = "01012345678",
     Gender = "Male",
        City = "Cairo"
    };

    // Act
    var result = await validator.ValidateAsync(request);

    // Assert
    Assert.IsTrue(result.IsValid);
}

[Test]
public async Task ValidatePhoneNumber_WithInvalidNumber_Fails()
{
    // Arrange
    var validator = new CustomerProfileValidator();
    var request = new UpdateCustomerProfileRequest
    {
        FullName = "Ahmed Hassan",
        PhoneNumber = "123456789", // Invalid
        Gender = "Male",
    City = "Cairo"
 };

    // Act
    var result = await validator.ValidateAsync(request);

  // Assert
    Assert.IsFalse(result.IsValid);
    Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "PhoneNumber"));
}
```

---

## 🚨 Common Mistakes to Avoid

### ❌ Mistake 1: Not checking ModelState before validation
```csharp
// Wrong:
var validationResult = await _validationService.ValidateCustomerProfileAsync(request);

// Right:
if (!ModelState.IsValid)
    return BadRequest(ModelState);

var validationResult = await _validationService.ValidateCustomerProfileAsync(request);
```

### ❌ Mistake 2: Ignoring validation errors
```csharp
// Wrong:
await _validationService.ValidateCustomerProfileAsync(request);
// Proceed without checking result

// Right:
var validationResult = await _validationService.ValidateCustomerProfileAsync(request);
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
   ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
    }
    return View(request);
}
```

### ❌ Mistake 3: Not validating file uploads
```csharp
// Wrong:
// Just accept any file

// Right:
RuleFor(x => x.ProfilePicture)
    .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
    .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت")
    .Must(IsValidImageFile)
    .WithMessage("نوع الملف غير مدعوم");
```

---

## 📊 Validation Result Properties

```csharp
ValidationResult result = await validator.ValidateAsync(request);

// Check if valid
bool isValid = result.IsValid;

// Get all errors
List<ValidationFailure> errors = result.Errors.ToList();

// Access specific error details
foreach (var error in result.Errors)
{
    string propertyName = error.PropertyName;  // e.g., "PhoneNumber"
string errorMessage = error.ErrorMessage;  // e.g., "رقم هاتف غير صحيح"
    object attemptedValue = error.AttemptedValue;
}

// Convert to dictionary (useful for API responses)
Dictionary<string, string[]> errorsDictionary = result.ToDictionary();
```

---

## 🔄 Integration with ASP.NET Core

### Display Errors in Razor Views:

```html
@model UpdateCustomerProfileRequest

<form asp-action="UpdateProfile" method="post">
    <div class="form-group">
        <label asp-for="FullName"></label>
        <input asp-for="FullName" class="form-control" />
        <span asp-validation-for="FullName" class="text-danger"></span>
    </div>

    <div class="form-group">
    <label asp-for="PhoneNumber"></label>
        <input asp-for="PhoneNumber" class="form-control" />
   <span asp-validation-for="PhoneNumber" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">حفظ</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## 📚 Additional Resources

**FluentValidation Documentation:**  
https://docs.fluentvalidation.net/

**Arabic Regex Patterns:**  
https://www.unicode.org/charts/PDF/U0600.pdf

**Egyptian Phone Number Format:**  
https://en.wikipedia.org/wiki/Telephone_numbers_in_Egypt

---

**Last Updated:** January 2025  
**Version:** 1.0  
**Maintainer:** Tafsilk Development Team
