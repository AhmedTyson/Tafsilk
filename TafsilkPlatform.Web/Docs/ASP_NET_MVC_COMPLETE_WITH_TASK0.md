
# TAFSILK PLATFORM - ASP.NET MVC DEVELOPMENT TASKS (REFINED - UPDATED)
## Complete Implementation Guide with Detailed Descriptions and Team Process

---

## EXECUTIVE SUMMARY

This document provides a comprehensive roadmap for completing the Tafsilk Platform ASP.NET MVC website. The project is divided into **4 major tasks** distributed among the development team:

| Task   | Owner | Priority | Focus Area                                        | Conditional Handoff                |
| ------ | ----- | -------- | ------------------------------------------------- | ---------------------------------- |
| Task 0 | Ahmed | CRITICAL | Profiles, Portfolios, Admin Dashboard, Validation | -                                  |
| Task 1 | Eriny | CRITICAL | Order Management System                           | -                                  |
| Task 2 | Eriny | HIGH     | Review & Rating, Portfolio Management             | If NOT complete → Omar takes 2 & 3 |
| Task 3 | Omar  | CRITICAL | Payment & Wallet Integration                      | Only if Task 2 NOT complete        |

**Execution Strategy:**

**Phase 0:** Ahmed builds Task 0 (Profiles, Admin) - Foundation for all features

**Phase 1:** Eriny builds Task 1 (Order Management) - Core business flow

**Phase 2:** Eriny attempts Task 2 (Reviews & Portfolio) - Customer feedback system
- If Eriny completes Task 2 fully: Proceed to integration testing
- If Eriny cannot complete Task 2: Omar takes over Task 2 & Task 3 (both Payment & remaining Reviews/Portfolio work)

**Phase 3:** Integration, testing, deployment

---

# DETAILED PREREQUISITES & SETUP

## Pre-Implementation Checklist

Before any developer starts coding, complete these steps:

### 1. Database Schema Validation

**Action Items:**
- [ ] Review `AppDbContext.cs` to confirm all Entity relationships are defined
- [ ] Verify all navigation properties are correctly set up (Customer ↔ Profile ↔ User)
- [ ] Confirm DbSet declarations exist for: CustomerProfile, TailorProfile, PortfolioImage, Admin, Review, Order, Payment, Wallet
- [ ] Run `dotnet ef migrations add Initial` to generate first migration
- [ ] Apply migration: `dotnet ef database update`
- [ ] Test database connection with simple query

**Owner:** Ahmed (Database Architect)
**Validation:** Run test query to confirm tables exist

---

### 2. Project Structure Setup

**Action Items:**
- [ ] Create folder structure:
  ```
  Controllers/
    - ProfilesController.cs (Ahmed)
    - AdminController.cs (Ahmed)
    - OrdersController.cs (Eriny)
    - ReviewsController.cs (Eriny)
    - PaymentsController.cs (Omar)

  Services/
    - ProfileService.cs (Ahmed)
    - AdminService.cs (Ahmed)
    - ValidationService.cs (Ahmed)
    - OrderService.cs (Eriny)
    - ReviewService.cs (Eriny)
    - PortfolioService.cs (Eriny)
    - PaymentService.cs (Omar)

  ViewModels/
    - Profiles/
      - CustomerProfileViewModel.cs
      - TailorProfileViewModel.cs
      - EditProfileRequest.cs
    - Admin/
      - AdminDashboardViewModel.cs
      - UserManagementViewModel.cs
    - Validation/
      - ProfileValidationRequest.cs
    - Orders/
      - CreateOrderViewModel.cs
      - OrderDetailsViewModel.cs
    - Reviews/
      - CreateReviewViewModel.cs
      - TailorReviewsViewModel.cs
    - Payments/
      - ProcessPaymentViewModel.cs
      - WalletManagementViewModel.cs

  Views/
    - Profiles/
      - CustomerProfile.cshtml
      - TailorProfile.cshtml
      - EditProfile.cshtml
    - Admin/
      - Dashboard.cshtml
      - UserManagement.cshtml
      - Analytics.cshtml
      - Verification.cshtml
    - Orders/ (Eriny)
      - Create.cshtml
      - Details.cshtml
      - CustomerIndex.cshtml
    - Reviews/ (Eriny)
      - Create.cshtml
      - TailorReviews.cshtml
    - Payments/ (Omar)
      - Process.cshtml
      - Success.cshtml

  Repositories/
    - (Update existing repos with new queries)
  ```

- [ ] Add NuGet packages if missing:
  ```
  dotnet add package Microsoft.EntityFrameworkCore
  dotnet add package Microsoft.AspNetCore.Identity
  dotnet add package Serilog
  dotnet add package FluentValidation
  ```

**Owner:** Ahmed (Architect)
**Validation:** All folders created, no errors on project build

---

### 3. Program.cs Foundation Setup

**Action Items:**
- [ ] Ensure dependency injection is registered for existing services
- [ ] Add placeholders for new service registrations (will be filled in by task owners)
- [ ] Verify authentication/authorization middleware is configured
- [ ] Test application startup: `dotnet run`

**Owner:** Ahmed

---

## Team Communication Protocol

### Daily Standup
- Each developer reports: Completed, Current, Blocked
- Ahmed monitors for cross-task blockers
- Escalate database changes immediately

### Code Review Process
- Ahmed submits Profile & Admin code for review
- Eriny submits Order Management code for Ahmed review
- Eriny submits Reviews code for Ahmed review
- Reviews completed within reasonable timeframe
- Fixes: within appropriate time frame of review feedback

### Git Workflow
```
Branch Naming: feature/task0-profiles, feature/task1-orders, feature/task2-reviews, feature/task3-payments
Commit Message: [Task0] ProfilesController: EditProfile() - Profile update implementation
Pull Request: Must pass code review before merge to main
```

---

# TASK 0: CUSTOMER & TAILOR PROFILES, PORTFOLIO SHOWCASE, ADMIN DASHBOARD & VALIDATION (AHMED'S RESPONSIBILITY)

## Overview

Task 0 is the **foundational infrastructure** for the entire Tafsilk Platform. It includes:
1. **Customer Profiles** - Store customer information, preferences, delivery addresses
2. **Tailor Profiles** - Showcase tailor expertise, location, services, availability
3. **Portfolio Showcase** - Display tailor work samples with before/after comparisons
4. **Profile Validation** - Comprehensive validation rules for all user types
5. **Admin Dashboard** - Complete platform management and monitoring interface

**Why This Task First?**
- All other features depend on complete user profiles
- Validation ensures data integrity across entire platform
- Admin dashboard needed for system monitoring and management
- Sets up authorization and role-based access control

**Success Criteria:**
- Customer profiles complete with all required information
- Tailor profiles fully searchable with ratings
- Portfolio system working with before/after comparisons
- All validation rules enforced on server and client side
- Admin dashboard shows real-time system metrics
- Role-based access control fully implemented

---

## PHASE 0.1: Planning & Design

### Entity Relationship Mapping

**Customer Profile Entities:**
```
CustomerProfile (Parent)
├── UserId (FK) → User
├── FullName, PhoneNumber, Gender
├── City, District, Preferences
├── UserAddresses[] (Multiple delivery addresses)
├── PaymentMethods[] (Saved payment methods)
├── SavedTailors[] (Favorite tailors)
└── Timestamps (CreatedAt, UpdatedAt)

UserAddress (Child)
├── AddressId (FK) → CustomerProfile
├── StreetAddress, City, District, PostalCode
├── IsDefault (Primary delivery address)
└── Label (Home, Work, etc.)
```

**Tailor Profile Entities:**
```
TailorProfile (Parent)
├── UserId (FK) → User
├── ShopName, Bio, PhoneNumber
├── Address, Latitude, Longitude
├── ExperienceYears, SkillLevel
├── AverageRating, ReviewCount
├── IsVerified, VerificationDate
├── Services[] (Tailoring services offered)
├── Availability (Weekly schedule)
├── PricingRange (Min/Max prices)
├── PortfolioImages[] (Work samples)
└── Timestamps

PortfolioImage (Child)
├── ImageId (FK) → TailorProfile
├── ImageUrl, Caption
├── IsBeforeAfter (Before/after comparison)
├── ServiceType, Garment
└── UploadedAt

TailorService (Child)
├── ServiceId (FK) → TailorProfile
├── ServiceName, Description, BasePrice
├── EstimatedDays, Required Materials
└── Active (Service is offered)
```

**Admin Entities:**
```
Admin (Parent)
├── UserId (FK) → User
├── AccessLevel (SuperAdmin, Admin, Moderator)
├── Permissions[] (Which features can manage)
└── Timestamps

AuditLog (Tracking)
├── LogId
├── UserId, Action, EntityType, EntityId
├── OldValue, NewValue
├── Timestamp

SystemMetric (Analytics)
├── MetricId
├── MetricType (Active Users, Orders, Revenue)
├── MetricValue, RecordedAt
```

---

### HTTP Route Planning

**Customer Profile Routes:**
```
GET    /profile/customer               → View customer profile
GET    /profile/customer/edit          → Edit customer profile form
POST   /profile/customer/edit          → Update customer profile
GET    /profile/addresses              → Manage delivery addresses
POST   /profile/addresses/add          → Add new address
PUT    /profile/addresses/{id}         → Update address
DELETE /profile/addresses/{id}         → Delete address
POST   /profile/favorite-tailors       → Save favorite tailor
GET    /profile/favorite-tailors       → View favorite tailors
```

**Tailor Profile Routes:**
```
GET    /profile/tailor                 → View tailor profile
GET    /profile/tailor/edit            → Edit tailor profile form
POST   /profile/tailor/edit            → Update tailor profile
GET    /profile/tailor/services        → Manage services offered
POST   /profile/tailor/services/add    → Add new service
PUT    /profile/tailor/services/{id}   → Update service
GET    /profile/tailor/availability    → Set availability schedule
PUT    /profile/tailor/availability    → Update availability
GET    /profile/tailor/{tailorId}      → View public tailor profile
```

**Portfolio Routes:**
```
GET    /portfolio/manage               → Tailor portfolio management (shares with Reviews Task 2)
POST   /portfolio/upload               → Upload portfolio image
DELETE /portfolio/{imageId}            → Delete portfolio image
GET    /tailors/search                 → Search tailors with filters
GET    /tailors/featured               → Featured tailors page
GET    /tailors/near-me                → Nearby tailors with geolocation
```

**Admin Routes:**
```
GET    /admin/dashboard                → Main dashboard with metrics
GET    /admin/users                    → User management page
GET    /admin/tailors/verification     → Tailor verification queue
POST   /admin/tailors/{id}/verify      → Approve tailor verification
POST   /admin/tailors/{id}/reject      → Reject tailor verification
GET    /admin/analytics                → System analytics and reports
GET    /admin/audit-log                → Audit trail
POST   /admin/users/{id}/suspend       → Suspend user account
DELETE /admin/users/{id}/ban           → Ban user account
```

---

### Data Flow Diagram

**Customer Profile Flow:**
```
Customer Signs Up
  ↓
System creates User entity
  ↓
System creates empty CustomerProfile
  ↓
Customer clicks "Complete Profile"
  ↓
ProfilesController.EditProfile() GET shows form
  ↓
Customer fills: Name, Phone, Gender, City, Preferences
  ↓
ProfilesController.EditProfile() POST validates
  ↓
ValidationService checks all rules
  ↓
If valid: ProfileService updates database
  ↓
Profile complete and searchable
  ↓
Customer can add multiple addresses
  ↓
Customer can save favorite tailors
```

**Tailor Verification Flow:**
```
Tailor Signs Up (User role = Tailor)
  ↓
System creates User entity
  ↓
System creates TailorProfile (IsVerified = false)
  ↓
Tailor completes profile (Shop name, experience, services)
  ↓
Tailor uploads portfolio images
  ↓
System flags for Admin verification
  ↓
Admin reviews in Dashboard
  ↓
Admin checks: Valid shop details, Quality portfolio, Compliance
  ↓
If approved: AdminService marks IsVerified = true
  ↓
Tailor profile visible to customers
  ↓
If rejected: AdminService sends rejection reason
  ↓
Tailor notified and can reapply
```

**Admin Dashboard Flow:**
```
Admin logs in
  ↓
AdminController.Dashboard() fetches real-time metrics
  ↓
Dashboard displays:
  - Active users count
  - Pending orders count
  - New tailor applications
  - System performance metrics
  - Recent transactions
  ↓
Admin can drill down into each section
  ↓
Admin can perform management actions
  ↓
AuditService logs every admin action
  ↓
Changes reflected in system
```

---

## PHASE 0.2: Controller Implementation

### ProfilesController.cs

**File:** `Controllers/ProfilesController.cs`

**Purpose:** Handle all user profile operations (Customer and Tailor)

---

#### Step 1: Class Declaration and Dependency Injection

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TafsilkPlatform.Models;
using TafsilkPlatform.Services;
using TafsilkPlatform.Data;
using TafsilkPlatform.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Controllers
{
    [Route("profile")]
    [ApiController]
    [Authorize]
    public class ProfilesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProfileService _profileService;
        private readonly IValidationService _validationService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(
            IUnitOfWork unitOfWork,
            IProfileService profileService,
            IValidationService validationService,
            IFileUploadService fileUploadService,
            ILogger<ProfilesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Action methods follow...
    }
}
```

---

#### Step 2: CustomerProfile View & Edit Actions

```csharp
/// <summary>
/// Displays customer's profile with all information.
/// </summary>
[HttpGet]
[Route("customer")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CustomerProfile()
{
    try
    {
        _logger.LogInformation($"Loading customer profile");

        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        // Fetch complete customer profile with all data
        var profile = await _unitOfWork.Customers
            .FirstOrDefaultAsync(c => c.UserId == customerGuid);

        if (profile == null)
        {
            _logger.LogWarning($"Customer profile not found for user {customerGuid}");
            return NotFound("Profile not found");
        }

        // Fetch addresses
        var addresses = await _unitOfWork.Addresses
            .Where(a => a.CustomerId == profile.Id)
            .ToListAsync();

        // Fetch saved tailors
        var savedTailors = await _unitOfWork.SavedTailors
            .Where(st => st.CustomerId == profile.Id)
            .Include(st => st.Tailor)
            .ToListAsync();

        var model = new CustomerProfileViewModel
        {
            CustomerId = profile.Id,
            UserId = customerGuid,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Gender = profile.Gender,
            City = profile.City,
            District = profile.District,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            Preferences = profile.Preferences,
            Addresses = addresses.Select(a => new AddressDto
            {
                AddressId = a.Id,
                Label = a.Label,
                StreetAddress = a.StreetAddress,
                City = a.City,
                District = a.District,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            }).ToList(),
            SavedTailors = savedTailors.Select(st => new SavedTailorDto
            {
                TailorId = st.TailorId,
                TailorName = st.Tailor?.ShopName,
                TailorRating = st.Tailor?.AverageRating ?? 0,
                SavedAt = st.SavedAt
            }).ToList(),
            AccountCreatedAt = profile.CreatedAt,
            LastUpdatedAt = profile.UpdatedAt
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading customer profile: {ex.Message}");
        return StatusCode(500, "Error loading profile");
    }
}

/// <summary>
/// Displays form to edit customer profile.
/// </summary>
[HttpGet]
[Route("customer/edit")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> EditCustomerProfile()
{
    try
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        var profile = await _unitOfWork.Customers
            .FirstOrDefaultAsync(c => c.UserId == customerGuid);

        if (profile == null)
            return NotFound("Profile not found");

        var model = new EditProfileRequest
        {
            CustomerId = profile.Id,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Gender = profile.Gender,
            City = profile.City,
            District = profile.District,
            Preferences = profile.Preferences
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading edit form: {ex.Message}");
        return StatusCode(500, "Error loading form");
    }
}

/// <summary>
/// Updates customer profile with validation.
/// </summary>
[HttpPost]
[Route("customer/edit")]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditCustomerProfile(EditProfileRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        // ==================== VALIDATION PHASE ====================

        // Validate all fields using ValidationService
        var validationResult = await _validationService.ValidateCustomerProfileAsync(request);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        // ==================== UPDATE PHASE ====================

        var result = await _profileService.UpdateCustomerProfileAsync(customerGuid, request);

        if (!result.Success)
        {
            _logger.LogError($"Profile update failed: {result.ErrorMessage}");
            ModelState.AddModelError("", result.ErrorMessage);
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Customer profile updated: {customerGuid}");

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(CustomerProfile));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception updating profile: {ex.Message}");
        ModelState.AddModelError("", $"Error: {ex.Message}");
        return View(request);
    }
}
```

---

#### Step 3: TailorProfile View & Edit Actions

```csharp
/// <summary>
/// Displays tailor's profile with services, availability, and portfolio info.
/// </summary>
[HttpGet]
[Route("tailor")]
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> TailorProfile()
{
    try
    {
        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        // Fetch tailor profile
        var profile = await _unitOfWork.Tailors
            .FirstOrDefaultAsync(t => t.UserId == tailorGuid);

        if (profile == null)
            return NotFound("Tailor profile not found");

        // Fetch services
        var services = await _unitOfWork.Services
            .Where(s => s.TailorId == profile.Id && s.IsActive)
            .ToListAsync();

        // Fetch portfolio count
        var portfolioCount = await _unitOfWork.Portfolio
            .CountAsync(p => p.TailorId == profile.Id);

        // Fetch recent reviews
        var recentReviews = await _unitOfWork.Reviews
            .Where(r => r.TailorId == profile.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var model = new TailorProfileViewModel
        {
            TailorId = profile.Id,
            UserId = tailorGuid,
            ShopName = profile.ShopName,
            Bio = profile.Bio,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            City = ExtractCity(profile.Address),
            District = profile.District,
            ExperienceYears = profile.ExperienceYears,
            SkillLevel = profile.SkillLevel, // Expert, Advanced, Intermediate
            AverageRating = profile.AverageRating,
            ReviewCount = profile.ReviewCount,
            IsVerified = profile.IsVerified,
            VerificationDate = profile.VerificationDate,
            PricingRange = profile.PricingRange,
            Services = services.Select(s => new ServiceDto
            {
                ServiceId = s.Id,
                ServiceName = s.ServiceName,
                Description = s.Description,
                BasePrice = s.BasePrice,
                EstimatedDays = s.EstimatedDays
            }).ToList(),
            PortfolioCount = portfolioCount,
            RecentReviews = recentReviews.Select(r => new ReviewSummaryDto
            {
                CustomerName = r.Customer?.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewedAt = r.CreatedAt
            }).ToList(),
            ProfilePictureUrl = profile.ProfilePictureUrl,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading tailor profile: {ex.Message}");
        return StatusCode(500, "Error loading profile");
    }
}

/// <summary>
/// Displays form to edit tailor profile.
/// </summary>
[HttpGet]
[Route("tailor/edit")]
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> EditTailorProfile()
{
    try
    {
        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        var profile = await _unitOfWork.Tailors
            .FirstOrDefaultAsync(t => t.UserId == tailorGuid);

        if (profile == null)
            return NotFound("Profile not found");

        var model = new EditProfileRequest
        {
            TailorId = profile.Id,
            ShopName = profile.ShopName,
            Bio = profile.Bio,
            PhoneNumber = profile.PhoneNumber,
            Address = profile.Address,
            ExperienceYears = profile.ExperienceYears,
            SkillLevel = profile.SkillLevel,
            PricingRange = profile.PricingRange
        };

        return View("EditTailorProfile", model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading edit form: {ex.Message}");
        return StatusCode(500, "Error loading form");
    }
}

/// <summary>
/// Updates tailor profile with validation.
/// </summary>
[HttpPost]
[Route("tailor/edit")]
[Authorize(Roles = "Tailor")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditTailorProfile(EditProfileRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        // ==================== VALIDATION PHASE ====================

        // Validate all fields using ValidationService
        var validationResult = await _validationService.ValidateTailorProfileAsync(request);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        // ==================== UPDATE PHASE ====================

        var result = await _profileService.UpdateTailorProfileAsync(tailorGuid, request);

        if (!result.Success)
        {
            _logger.LogError($"Profile update failed: {result.ErrorMessage}");
            ModelState.AddModelError("", result.ErrorMessage);
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Tailor profile updated: {tailorGuid}");

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(TailorProfile));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception updating profile: {ex.Message}");
        ModelState.AddModelError("", $"Error: {ex.Message}");
        return View(request);
    }
}
```

---

#### Step 4: Address Management Actions

```csharp
/// <summary>
/// Displays all customer delivery addresses.
/// </summary>
[HttpGet]
[Route("addresses")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> ManageAddresses()
{
    try
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        var profile = await _unitOfWork.Customers
            .FirstOrDefaultAsync(c => c.UserId == customerGuid);

        if (profile == null)
            return NotFound("Profile not found");

        var addresses = await _unitOfWork.Addresses
            .Where(a => a.CustomerId == profile.Id)
            .ToListAsync();

        var model = new AddressManagementViewModel
        {
            CustomerId = profile.Id,
            Addresses = addresses.Select(a => new AddressDto
            {
                AddressId = a.Id,
                Label = a.Label,
                StreetAddress = a.StreetAddress,
                City = a.City,
                District = a.District,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            }).ToList()
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading addresses: {ex.Message}");
        return StatusCode(500, "Error loading addresses");
    }
}

/// <summary>
/// Adds new delivery address with validation.
/// </summary>
[HttpPost]
[Route("addresses/add")]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddAddress(AddAddressRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        // ==================== VALIDATION ====================

        var validationResult = await _validationService.ValidateAddressAsync(request);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        // ==================== CREATE ADDRESS ====================

        var result = await _profileService.AddAddressAsync(customerGuid, request);

        if (!result.Success)
        {
            _logger.LogError($"Failed to add address: {result.ErrorMessage}");
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation($"Address added for customer {customerGuid}");

        return RedirectToAction(nameof(ManageAddresses));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception adding address: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}

/// <summary>
/// Deletes a delivery address.
/// </summary>
[HttpPost]
[Route("addresses/{addressId:guid}/delete")]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteAddress(Guid addressId)
{
    try
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        var result = await _profileService.DeleteAddressAsync(customerGuid, addressId);

        if (!result.Success)
        {
            _logger.LogError($"Failed to delete address: {result.ErrorMessage}");
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation($"Address deleted: {addressId}");

        return RedirectToAction(nameof(ManageAddresses));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception deleting address: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

#### Step 5: Tailor Services Management

```csharp
/// <summary>
/// Displays tailor's offered services.
/// </summary>
[HttpGet]
[Route("tailor/services")]
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> ManageServices()
{
    try
    {
        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        var profile = await _unitOfWork.Tailors
            .FirstOrDefaultAsync(t => t.UserId == tailorGuid);

        if (profile == null)
            return NotFound("Profile not found");

        var services = await _unitOfWork.Services
            .Where(s => s.TailorId == profile.Id)
            .ToListAsync();

        var model = new ServiceManagementViewModel
        {
            TailorId = profile.Id,
            Services = services.Select(s => new ServiceDto
            {
                ServiceId = s.Id,
                ServiceName = s.ServiceName,
                Description = s.Description,
                BasePrice = s.BasePrice,
                EstimatedDays = s.EstimatedDays,
                IsActive = s.IsActive
            }).ToList()
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading services: {ex.Message}");
        return StatusCode(500, "Error loading services");
    }
}

/// <summary>
/// Adds new service with validation.
/// </summary>
[HttpPost]
[Route("tailor/services/add")]
[Authorize(Roles = "Tailor")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddService(AddServiceRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        // ==================== VALIDATION ====================

        var validationResult = await _validationService.ValidateServiceAsync(request);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        // ==================== CREATE SERVICE ====================

        var result = await _profileService.AddServiceAsync(tailorGuid, request);

        if (!result.Success)
        {
            _logger.LogError($"Failed to add service: {result.ErrorMessage}");
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation($"Service added for tailor {tailorGuid}");

        return RedirectToAction(nameof(ManageServices));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception adding service: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

#### Step 6: Public Tailor Search & Discovery

```csharp
/// <summary>
/// Search and filter verified tailors by location and services.
/// </summary>
[HttpGet]
[Route("search-tailors")]
[AllowAnonymous]
public async Task<IActionResult> SearchTailors(
    string? city = null,
    string? district = null,
    string? serviceType = null,
    decimal? minRating = null,
    int page = 1)
{
    try
    {
        _logger.LogInformation($"Searching tailors: city={city}, serviceType={serviceType}");

        // ==================== BUILD QUERY ====================

        var query = _unitOfWork.Tailors
            .Where(t => t.IsVerified); // Only show verified tailors

        // Apply city filter
        if (!string.IsNullOrEmpty(city))
            query = query.Where(t => t.Address.Contains(city));

        // Apply district filter
        if (!string.IsNullOrEmpty(district))
            query = query.Where(t => t.District == district);

        // Apply minimum rating filter
        if (minRating.HasValue)
            query = query.Where(t => t.AverageRating >= minRating.Value);

        // ==================== PAGINATION ====================

        const int pageSize = 12;
        var totalCount = await query.CountAsync();
        var tailors = await query
            .OrderByDescending(t => t.AverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ==================== FILTER BY SERVICE (if specified) ====================

        if (!string.IsNullOrEmpty(serviceType))
        {
            tailors = tailors
                .Where(t => t.Services.Any(s => s.ServiceName.Contains(serviceType)))
                .ToList();
        }

        // ==================== BUILD MODEL ====================

        var model = new TailorSearchViewModel
        {
            Tailors = tailors.Select(t => new TailorSearchResultDto
            {
                TailorId = t.Id,
                ShopName = t.ShopName,
                Address = t.Address,
                City = ExtractCity(t.Address),
                District = t.District,
                AverageRating = t.AverageRating,
                ReviewCount = t.ReviewCount,
                ExperienceYears = t.ExperienceYears,
                Services = t.Services
                    .Where(s => s.IsActive)
                    .Select(s => s.ServiceName)
                    .ToList(),
                ProfilePictureUrl = t.ProfilePictureUrl,
                VerifiedBadge = t.IsVerified
            }).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            CurrentCity = city,
            CurrentDistrict = district,
            CurrentServiceType = serviceType,
            TotalResults = totalCount
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error searching tailors: {ex.Message}");
        return StatusCode(500, "Error searching tailors");
    }
}

/// <summary>
/// View complete public tailor profile.
/// </summary>
[HttpGet]
[Route("tailor/{tailorId:guid}")]
[AllowAnonymous]
public async Task<IActionResult> ViewPublicTailorProfile(Guid tailorId)
{
    try
    {
        var profile = await _unitOfWork.Tailors
            .FirstOrDefaultAsync(t => t.Id == tailorId);

        if (profile == null || !profile.IsVerified)
            return NotFound("Tailor not found");

        // Fetch services
        var services = await _unitOfWork.Services
            .Where(s => s.TailorId == tailorId && s.IsActive)
            .ToListAsync();

        // Fetch portfolio images
        var portfolioImages = await _unitOfWork.Portfolio
            .Where(p => p.TailorId == tailorId)
            .ToListAsync();

        // Fetch reviews
        var reviews = await _unitOfWork.Reviews
            .Where(r => r.TailorId == tailorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var model = new PublicTailorProfileViewModel
        {
            TailorId = profile.Id,
            ShopName = profile.ShopName,
            Bio = profile.Bio,
            Address = profile.Address,
            ExperienceYears = profile.ExperienceYears,
            AverageRating = profile.AverageRating,
            ReviewCount = profile.ReviewCount,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            VerificationDate = profile.VerificationDate,
            Services = services.Select(s => new ServiceDto
            {
                ServiceId = s.Id,
                ServiceName = s.ServiceName,
                Description = s.Description,
                BasePrice = s.BasePrice,
                EstimatedDays = s.EstimatedDays
            }).ToList(),
            PortfolioImages = portfolioImages.Select(p => new PortfolioImageDto
            {
                ImageId = p.Id,
                ImageUrl = p.ImageUrl,
                IsBeforeAfter = p.IsBeforeAfter,
                Caption = p.Caption
            }).ToList(),
            RecentReviews = reviews.Take(10).Select(r => new ReviewDisplayDto
            {
                CustomerName = r.Customer?.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading public profile: {ex.Message}");
        return StatusCode(500, "Error loading profile");
    }
}
```

---

## PHASE 0.3: Admin Controller Implementation

### AdminController.cs

**File:** `Controllers/AdminController.cs`

**Purpose:** Handle all admin dashboard and management operations

---

#### Step 1: Admin Dashboard

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TafsilkPlatform.Models;
using TafsilkPlatform.Services;
using TafsilkPlatform.Data;
using TafsilkPlatform.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUnitOfWork unitOfWork,
            IAdminService adminService,
            ILogger<AdminController> logger)
        {
            _unitOfWork = unitOfWork;
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Main admin dashboard with real-time metrics.
        /// </summary>
        [HttpGet]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                _logger.LogInformation("Loading admin dashboard");

                // Fetch real-time metrics
                var totalUsers = await _unitOfWork.Users.CountAsync();
                var totalCustomers = await _unitOfWork.Customers.CountAsync();
                var totalTailors = await _unitOfWork.Tailors.CountAsync();
                var verifiedTailors = await _unitOfWork.Tailors
                    .CountAsync(t => t.IsVerified);
                var pendingVerifications = await _unitOfWork.Tailors
                    .CountAsync(t => !t.IsVerified && t.CreatedAt > DateTime.UtcNow.AddDays(-7));
                var totalOrders = await _unitOfWork.Orders.CountAsync();
                var activeOrders = await _unitOfWork.Orders
                    .CountAsync(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
                var totalTransactions = await _unitOfWork.Payments.CountAsync();
                var totalRevenue = await _unitOfWork.Payments
                    .Where(p => p.PaymentStatus == "Completed")
                    .SumAsync(p => p.Amount);

                // Fetch recent activities
                var recentOrders = await _unitOfWork.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var recentReviews = await _unitOfWork.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var recentSignups = await _unitOfWork.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Build model
                var model = new AdminDashboardViewModel
                {
                    // Metrics
                    TotalUsers = totalUsers,
                    TotalCustomers = totalCustomers,
                    TotalTailors = totalTailors,
                    VerifiedTailors = verifiedTailors,
                    PendingVerifications = pendingVerifications,
                    TotalOrders = totalOrders,
                    ActiveOrders = activeOrders,
                    TotalTransactions = totalTransactions,
                    TotalRevenue = totalRevenue,

                    // Recent activities
                    RecentOrders = recentOrders.Select(o => new OrderSummaryDto
                    {
                        OrderId = o.Id,
                        OrderNumber = o.Id.ToString().Substring(0, 8),
                        CustomerName = o.Customer?.Name,
                        TailorName = o.Tailor?.ShopName,
                        Status = o.Status,
                        Price = o.TotalPrice,
                        CreatedAt = o.CreatedAt
                    }).ToList(),

                    RecentReviews = recentReviews.Select(r => new ReviewSummaryDto
                    {
                        ReviewId = r.Id,
                        CustomerName = r.Customer?.Name,
                        TailorName = r.Tailor?.ShopName,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt
                    }).ToList(),

                    RecentSignups = recentSignups.Select(u => new UserSummaryDto
                    {
                        UserId = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.RoleId,
                        SignedUpAt = u.CreatedAt
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard: {ex.Message}");
                return StatusCode(500, "Error loading dashboard");
            }
        }
    }
}
```

---

#### Step 2: Tailor Verification Management

```csharp
/// <summary>
/// View all pending tailor verifications.
/// </summary>
[HttpGet]
[Route("tailors/verification")]
public async Task<IActionResult> TailorVerification(int page = 1)
{
    try
    {
        _logger.LogInformation("Loading tailor verification queue");

        // Fetch unverified tailors ordered by signup date
        const int pageSize = 10;
        var query = _unitOfWork.Tailors
            .Where(t => !t.IsVerified)
            .OrderBy(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var tailors = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Fetch additional info for each tailor
        var verification = new List<TailorVerificationDto>();
        foreach (var tailor in tailors)
        {
            var services = await _unitOfWork.Services
                .Where(s => s.TailorId == tailor.Id)
                .CountAsync();

            var portfolioCount = await _unitOfWork.Portfolio
                .CountAsync(p => p.TailorId == tailor.Id);

            verification.Add(new TailorVerificationDto
            {
                TailorId = tailor.Id,
                ShopName = tailor.ShopName,
                Bio = tailor.Bio,
                Address = tailor.Address,
                ExperienceYears = tailor.ExperienceYears,
                ServicesCount = services,
                PortfolioCount = portfolioCount,
                SubmittedAt = tailor.CreatedAt,
                ProfilePictureUrl = tailor.ProfilePictureUrl
            });
        }

        var model = new VerificationQueueViewModel
        {
            PendingTailors = verification,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalPending = totalCount
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading verification queue: {ex.Message}");
        return StatusCode(500, "Error loading queue");
    }
}

/// <summary>
/// Approve a tailor profile for verification.
/// </summary>
[HttpPost]
[Route("tailors/{tailorId:guid}/verify")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> VerifyTailor(Guid tailorId)
{
    try
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminId, out var adminGuid))
            return Unauthorized();

        var result = await _adminService.VerifyTailorAsync(tailorId, adminGuid);

        if (!result.Success)
        {
            _logger.LogError($"Verification failed: {result.ErrorMessage}");
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation($"Tailor {tailorId} verified by admin {adminGuid}");

        TempData["SuccessMessage"] = "Tailor verified successfully!";
        return RedirectToAction(nameof(TailorVerification));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception verifying tailor: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}

/// <summary>
/// Reject a tailor profile verification.
/// </summary>
[HttpPost]
[Route("tailors/{tailorId:guid}/reject")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RejectTailor(Guid tailorId, string? reason = null)
{
    try
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminId, out var adminGuid))
            return Unauthorized();

        var result = await _adminService.RejectTailorAsync(tailorId, adminGuid, reason);

        if (!result.Success)
        {
            _logger.LogError($"Rejection failed: {result.ErrorMessage}");
            return BadRequest(result.ErrorMessage);
        }

        _logger.LogInformation($"Tailor {tailorId} rejected by admin {adminGuid}");

        TempData["WarningMessage"] = "Tailor verification rejected!";
        return RedirectToAction(nameof(TailorVerification));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception rejecting tailor: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

#### Step 3: User Management

```csharp
/// <summary>
/// View all users with filtering and pagination.
/// </summary>
[HttpGet]
[Route("users")]
public async Task<IActionResult> UserManagement(string? role = null, string? searchTerm = null, int page = 1)
{
    try
    {
        _logger.LogInformation($"Loading user management: role={role}, search={searchTerm}");

        var query = _unitOfWork.Users.AsQueryable();

        // Filter by role if specified
        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role.Name == role);

        // Search by name or email
        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));

        const int pageSize = 20;
        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new UserManagementViewModel
        {
            Users = users.Select(u => new UserSummaryDto
            {
                UserId = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role?.Name,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            }).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalUsers = totalCount,
            CurrentRole = role,
            SearchTerm = searchTerm
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading user management: {ex.Message}");
        return StatusCode(500, "Error loading users");
    }
}

/// <summary>
/// Suspend user account.
/// </summary>
[HttpPost]
[Route("users/{userId:guid}/suspend")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SuspendUser(Guid userId, string? reason = null)
{
    try
    {
        var result = await _adminService.SuspendUserAsync(userId, reason);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        _logger.LogInformation($"User {userId} suspended");

        return RedirectToAction(nameof(UserManagement));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error suspending user: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}

/// <summary>
/// Ban user account permanently.
/// </summary>
[HttpPost]
[Route("users/{userId:guid}/ban")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> BanUser(Guid userId, string? reason = null)
{
    try
    {
        var result = await _adminService.BanUserAsync(userId, reason);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        _logger.LogInformation($"User {userId} banned");

        return RedirectToAction(nameof(UserManagement));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error banning user: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

## PHASE 0.4: Service Layer Implementation

### ProfileService.cs

```csharp
using Microsoft.Extensions.Logging;
using TafsilkPlatform.Data;
using TafsilkPlatform.Models;
using TafsilkPlatform.ViewModels;

namespace TafsilkPlatform.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IUnitOfWork unitOfWork,
            IFileUploadService fileUploadService,
            ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        /// <summary>
        /// Updates customer profile with validation.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateCustomerProfileAsync(
            Guid customerId,
            EditProfileRequest request)
        {
            try
            {
                _logger.LogInformation($"[ProfileService] Updating customer profile: {customerId}");

                var profile = await _unitOfWork.Customers
                    .FirstOrDefaultAsync(c => c.UserId == customerId);

                if (profile == null)
                    return (false, "Profile not found");

                // ==================== UPDATE FIELDS ====================

                profile.FullName = request.FullName;
                profile.PhoneNumber = request.PhoneNumber;
                profile.Gender = request.Gender;
                profile.City = request.City;
                profile.District = request.District;
                profile.Preferences = request.Preferences;
                profile.UpdatedAt = DateTime.UtcNow;

                // ==================== SAVE CHANGES ====================

                await _unitOfWork.Customers.UpdateAsync(profile);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[ProfileService] Customer profile updated");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProfileService] Error: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates tailor profile with validation.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateTailorProfileAsync(
            Guid tailorId,
            EditProfileRequest request)
        {
            try
            {
                _logger.LogInformation($"[ProfileService] Updating tailor profile: {tailorId}");

                var profile = await _unitOfWork.Tailors
                    .FirstOrDefaultAsync(t => t.UserId == tailorId);

                if (profile == null)
                    return (false, "Profile not found");

                // ==================== UPDATE FIELDS ====================

                profile.ShopName = request.ShopName;
                profile.Bio = request.Bio;
                profile.PhoneNumber = request.PhoneNumber;
                profile.Address = request.Address;
                profile.ExperienceYears = request.ExperienceYears;
                profile.SkillLevel = request.SkillLevel;
                profile.PricingRange = request.PricingRange;
                profile.UpdatedAt = DateTime.UtcNow;

                // ==================== SAVE CHANGES ====================

                await _unitOfWork.Tailors.UpdateAsync(profile);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[ProfileService] Tailor profile updated");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProfileService] Error: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds new delivery address for customer.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AddAddressAsync(
            Guid customerId,
            AddAddressRequest request)
        {
            try
            {
                _logger.LogInformation($"[ProfileService] Adding address for customer: {customerId}");

                var profile = await _unitOfWork.Customers
                    .FirstOrDefaultAsync(c => c.UserId == customerId);

                if (profile == null)
                    return (false, "Customer profile not found");

                var address = new UserAddress
                {
                    Id = Guid.NewGuid(),
                    CustomerId = profile.Id,
                    Label = request.Label,
                    StreetAddress = request.StreetAddress,
                    City = request.City,
                    District = request.District,
                    PostalCode = request.PostalCode,
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.UtcNow
                };

                // If this is the default address, unset other defaults
                if (request.IsDefault)
                {
                    var existingAddresses = await _unitOfWork.Addresses
                        .Where(a => a.CustomerId == profile.Id)
                        .ToListAsync();

                    foreach (var existing in existingAddresses)
                    {
                        existing.IsDefault = false;
                    }
                }

                await _unitOfWork.Addresses.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[ProfileService] Address added: {address.Id}");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProfileService] Error adding address: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a delivery address.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteAddressAsync(
            Guid customerId,
            Guid addressId)
        {
            try
            {
                _logger.LogInformation($"[ProfileService] Deleting address: {addressId}");

                var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);

                if (address == null)
                    return (false, "Address not found");

                // Verify ownership
                var profile = await _unitOfWork.Customers
                    .FirstOrDefaultAsync(c => c.UserId == customerId);

                if (profile == null || address.CustomerId != profile.Id)
                    return (false, "Unauthorized");

                await _unitOfWork.Addresses.DeleteAsync(address);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[ProfileService] Address deleted");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProfileService] Error deleting address: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds new service for tailor.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AddServiceAsync(
            Guid tailorId,
            AddServiceRequest request)
        {
            try
            {
                _logger.LogInformation($"[ProfileService] Adding service for tailor: {tailorId}");

                var profile = await _unitOfWork.Tailors
                    .FirstOrDefaultAsync(t => t.UserId == tailorId);

                if (profile == null)
                    return (false, "Tailor profile not found");

                var service = new TailorService
                {
                    Id = Guid.NewGuid(),
                    TailorId = profile.Id,
                    ServiceName = request.ServiceName,
                    Description = request.Description,
                    BasePrice = request.BasePrice,
                    EstimatedDays = request.EstimatedDays,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Services.AddAsync(service);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[ProfileService] Service added: {service.Id}");

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProfileService] Error adding service: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
```

---

### ValidationService.cs

```csharp
using FluentValidation;
using FluentValidation.Results;
using TafsilkPlatform.ViewModels;

namespace TafsilkPlatform.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates customer profile data.
        /// </summary>
        public async Task<ValidationResult> ValidateCustomerProfileAsync(EditProfileRequest request)
        {
            _logger.LogInformation($"[ValidationService] Validating customer profile");

            var validator = new CustomerProfileValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning($"Validation error: {error.PropertyName} - {error.ErrorMessage}");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates tailor profile data.
        /// </summary>
        public async Task<ValidationResult> ValidateTailorProfileAsync(EditProfileRequest request)
        {
            _logger.LogInformation($"[ValidationService] Validating tailor profile");

            var validator = new TailorProfileValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning($"Validation error: {error.PropertyName} - {error.ErrorMessage}");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates address data.
        /// </summary>
        public async Task<ValidationResult> ValidateAddressAsync(AddAddressRequest request)
        {
            _logger.LogInformation($"[ValidationService] Validating address");

            var validator = new AddressValidator();
            var result = await validator.ValidateAsync(request);

            return result;
        }

        /// <summary>
        /// Validates service data.
        /// </summary>
        public async Task<ValidationResult> ValidateServiceAsync(AddServiceRequest request)
        {
            _logger.LogInformation($"[ValidationService] Validating service");

            var validator = new ServiceValidator();
            var result = await validator.ValidateAsync(request);

            return result;
        }
    }

    // ==================== VALIDATORS ====================

    public class CustomerProfileValidator : AbstractValidator<EditProfileRequest>
    {
        public CustomerProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^01[0-2,5]\d{8}$").WithMessage("Invalid Egyptian phone number format");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(x => x == "Male" || x == "Female").WithMessage("Invalid gender");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required");

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required");

            RuleFor(x => x.Preferences)
                .MaximumLength(500).WithMessage("Preferences cannot exceed 500 characters");
        }
    }

    public class TailorProfileValidator : AbstractValidator<EditProfileRequest>
    {
        public TailorProfileValidator()
        {
            RuleFor(x => x.ShopName)
                .NotEmpty().WithMessage("Shop name is required")
                .MinimumLength(3).WithMessage("Shop name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Shop name cannot exceed 100 characters");

            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Bio is required")
                .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^01[0-2,5]\d{8}$").WithMessage("Invalid Egyptian phone number format");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MaximumLength(255).WithMessage("Address cannot exceed 255 characters");

            RuleFor(x => x.ExperienceYears)
                .GreaterThan(0).WithMessage("Experience years must be greater than 0")
                .LessThan(60).WithMessage("Experience years cannot exceed 60");

            RuleFor(x => x.SkillLevel)
                .NotEmpty().WithMessage("Skill level is required")
                .Must(x => x == "Intermediate" || x == "Advanced" || x == "Expert")
                .WithMessage("Invalid skill level");

            RuleFor(x => x.PricingRange)
                .NotEmpty().WithMessage("Pricing range is required");
        }
    }

    public class AddressValidator : AbstractValidator<AddAddressRequest>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Address label is required")
                .MaximumLength(50).WithMessage("Label cannot exceed 50 characters");

            RuleFor(x => x.StreetAddress)
                .NotEmpty().WithMessage("Street address is required")
                .MaximumLength(255).WithMessage("Street address cannot exceed 255 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required");

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("District is required");

            RuleFor(x => x.PostalCode)
                .MaximumLength(10).WithMessage("Postal code cannot exceed 10 characters");
        }
    }

    public class ServiceValidator : AbstractValidator<AddServiceRequest>
    {
        public ServiceValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("Service name is required")
                .MaximumLength(100).WithMessage("Service name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(10000).WithMessage("Price cannot exceed 10,000 EGP");

            RuleFor(x => x.EstimatedDays)
                .GreaterThan(0).WithMessage("Estimated days must be at least 1")
                .LessThan(100).WithMessage("Estimated days cannot exceed 100");
        }
    }
}
```

---

### AdminService.cs

```csharp
public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<AdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Approves tailor verification.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> VerifyTailorAsync(
        Guid tailorId,
        Guid adminId)
    {
        try
        {
            _logger.LogInformation($"[AdminService] Verifying tailor: {tailorId}");

            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
                return (false, "Tailor not found");

            tailor.IsVerified = true;
            tailor.VerificationDate = DateTime.UtcNow;

            await _unitOfWork.Tailors.UpdateAsync(tailor);
            await _unitOfWork.SaveChangesAsync();

            // Notify tailor
            await _notificationService.NotifyTailorVerificationAsync(
                tailor.UserId,
                true,
                null
            );

            _logger.LogInformation($"[AdminService] Tailor verified successfully");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[AdminService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Rejects tailor verification.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> RejectTailorAsync(
        Guid tailorId,
        Guid adminId,
        string? reason = null)
    {
        try
        {
            _logger.LogInformation($"[AdminService] Rejecting tailor: {tailorId}");

            var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
            if (tailor == null)
                return (false, "Tailor not found");

            tailor.VerificationNotes = reason;
            tailor.RejectionCount = (tailor.RejectionCount ?? 0) + 1;

            await _unitOfWork.Tailors.UpdateAsync(tailor);
            await _unitOfWork.SaveChangesAsync();

            // Notify tailor
            await _notificationService.NotifyTailorVerificationAsync(
                tailor.UserId,
                false,
                reason
            );

            _logger.LogInformation($"[AdminService] Tailor rejected");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[AdminService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Suspends user account.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> SuspendUserAsync(
        Guid userId,
        string? reason = null)
    {
        try
        {
            _logger.LogInformation($"[AdminService] Suspending user: {userId}");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found");

            user.IsActive = false;
            user.SuspensionReason = reason;
            user.SuspendedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[AdminService] User suspended");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[AdminService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Permanently bans user account.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> BanUserAsync(
        Guid userId,
        string? reason = null)
    {
        try
        {
            _logger.LogInformation($"[AdminService] Banning user: {userId}");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, "User not found");

            user.IsBanned = true;
            user.BanReason = reason;
            user.BannedAt = DateTime.UtcNow;
            user.IsActive = false;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[AdminService] User banned");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[AdminService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }
}
```

---

## PHASE 0.5: View Implementation

### Views/Profiles/CustomerProfile.cshtml
- Display customer information
- Edit button for own profile
- Manage addresses section
- Saved tailors display
- Order history links

### Views/Profiles/TailorProfile.cshtml (Tailor View)
- Display shop information
- Services offered
- Portfolio count
- Recent reviews preview
- Edit button for own profile
- Manage services link

### Views/Profiles/TailorProfile.cshtml (Public View)
- Shop information (read-only)
- Services offered with prices
- Portfolio gallery
- Reviews and ratings
- "Book Now" button (for customers)
- Contact information

### Views/Admin/Dashboard.cshtml
- System metrics in cards (Users, Orders, Revenue)
- Charts (orders by day, revenue trend)
- Recent orders table
- Pending verifications count
- Recent reviews
- Recent signups

### Views/Admin/TailorVerification.cshtml
- Queue of pending tailors
- Portfolio preview
- Services count
- Approve/Reject buttons
- Pagination

### Views/Admin/UserManagement.cshtml
- User table with filtering
- Role column
- Status indicator
- Actions: View, Suspend, Ban
- Search bar
- Pagination

---

## PHASE 0.6: Program.cs Registration

```csharp
// In Program.cs, add:

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Services
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
```

---

## PHASE 0.7: Testing & Validation

### Integration Tests

```csharp
[TestFixture]
public class ProfileControllerTests
{
    private ProfilesController _controller;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IProfileService> _mockProfileService;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProfileService = new Mock<IProfileService>();
        _controller = new ProfilesController(
            _mockUnitOfWork.Object,
            _mockProfileService.Object,
            Mock.Of<IValidationService>(),
            Mock.Of<IFileUploadService>(),
            Mock.Of<ILogger<ProfilesController>>()
        );
    }

    [Test]
    public async Task EditCustomerProfile_WithValidRequest_UpdatesProfile()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new EditProfileRequest
        {
            FullName = "Ahmed Hassan",
            PhoneNumber = "01001234567",
            City = "Cairo",
            District = "Heliopolis"
        };

        _mockProfileService
            .Setup(s => s.UpdateCustomerProfileAsync(It.IsAny<Guid>(), It.IsAny<EditProfileRequest>()))
            .ReturnsAsync((true, null));

        // Act
        var result = await _controller.EditCustomerProfile(request);

        // Assert
        Assert.IsInstanceOf<RedirectToActionResult>(result);
    }
}
```

---

## PHASE 0.8: Code Review Checklist

Before submitting for review:

- [ ] All customer profile fields validated
- [ ] All tailor profile fields validated
- [ ] All address fields validated
- [ ] All service fields validated
- [ ] Authorization checks on all endpoints
- [ ] CSRF token validation on POST endpoints
- [ ] Proper use of async/await
- [ ] Error handling with try-catch
- [ ] Logging at appropriate levels
- [ ] SQL injection prevention
- [ ] XSS protection
- [ ] Database queries optimized
- [ ] Views properly structured
- [ ] Admin dashboard shows all metrics
- [ ] Verification workflow complete

---

## TASK 0 SUMMARY

**Deliverables:**
- ProfilesController with customer/tailor management
- AdminController with dashboard and verification
- ProfileService with business logic
- ValidationService with comprehensive rules
- AdminService for user management
- Fluent Validation validators for all entities
- 8+ Razor views for profiles and admin
- Complete repository methods
- ViewModels for data binding
- Integration tests
- Documentation

**Note:** This task DOES NOT include order management for tailors - that will be handled by Eriny in Task 1.

---
----
---

## Team Communication Protocol

### Daily Standup
- Each developer reports: Completed, Current, Blocked
- Ahmed monitors for cross-task blockers
- Escalate database changes immediately

### Code Review Process
- Eriny submits Order Management code for Ahmed review
- Eriny submits Reviews code for Ahmed review
- Reviews completed within reasonable timeframe
- Fixes: within appropriate time frame of review feedback

### Git Workflow
```
Branch Naming: feature/task1-orders, feature/task2-reviews, feature/task3-payments
Commit Message: [Task1] OrdersController: CreateOrder() - Image upload implementation
Pull Request: Must pass code review before merge to main
```

---

# TASK 1: ORDER MANAGEMENT SYSTEM - DETAILED BREAKDOWN

## Overview

The Order Management System is the **foundation** of the Tafsilk Platform. It manages the complete lifecycle of customer requests: from order creation through tailor completion. This task encompasses order creation, tracking, status management, and order history retrieval for both customers and tailors.

**Why This Task First?**
- Payment and Review systems depend on Order entities
- Establishes core business flow
- Enables testing of other systems once complete

**Success Criteria:**
- Customer can create order in reasonable timeframe
- Tailor receives real-time notifications
- Status changes trigger automatic notifications
- 100% order history retrieval accuracy

---

## PHASE 1.1: Planning & Design

### Step 1: Entity Relationship Mapping

**Task:** Understand how Order entity connects to other entities

**Entities Involved:**
```
Order (Parent)
├── CustomerId (FK) → User
├── TailorId (FK) → User/TailorProfile
├── OrderItems[] (Line items)
├── OrderImages[] (Customer-uploaded images)
├── OrderStatusHistory[] (Timeline of status changes)
├── OrderMessages[] (Chat between customer and tailor)
├── Quote (Tailor's price quote)
└── Payment (Associated payment record)
```

**Database Relationships to Verify:**
- One Order → Many OrderImages (1:N)
- One Order → Many OrderStatusHistory (1:N)
- One Order → One Quote (1:1)
- One Order → One Payment (1:1)
- One Order → Many OrderMessages (1:N)

**Action:**
```csharp
// In AppDbContext.cs - verify these relationships exist
modelBuilder.Entity<Order>()
    .HasMany(o => o.OrderImages)
    .WithOne()
    .HasForeignKey(oi => oi.OrderId);

modelBuilder.Entity<Order>()
    .HasMany(o => o.StatusHistory)
    .WithOne()
    .HasForeignKey(sh => sh.OrderId);

modelBuilder.Entity<Order>()
    .HasOne(o => o.Quote)
    .WithOne()
    .HasForeignKey<Quote>(q => q.OrderId);
```

**Owner:** Eriny + Ahmed
**Deliverable:** Confirmation that all relationships are correctly configured

---

### Step 2: HTTP Route Planning

**Task:** Define all HTTP endpoints the controller will handle

**Routes to Implement:**
```
GET    /orders/create                     → CreateOrder() form page
POST   /orders/create                     → Process order submission
GET    /orders/details/{orderId}          → Display order with tracking
POST   /orders/update-status              → Tailor updates status
GET    /orders/my-orders                  → Customer order history
GET    /orders/my-orders?filter=active    → Filter by status
GET    /orders/my-orders-tailor           → Tailor's order list
```

**Authorization Strategy:**
```
GET /orders/create              → [Authorize(Roles = "Customer")]
POST /orders/create             → [Authorize(Roles = "Customer")]
GET /orders/details/{id}        → Allow Customer OR Tailor OR Admin
POST /orders/update-status      → [Authorize(Roles = "Tailor")] only
GET /orders/my-orders           → [Authorize(Roles = "Customer")]
GET /orders/my-orders-tailor    → [Authorize(Roles = "Tailor")]
```

**Owner:** Eriny
**Deliverable:** Documented route plan

---

### Step 3: Data Flow Diagram

**Task:** Visualize the complete order creation and tracking flow

**Flow:**
```
Customer clicks "Create Order"
  ↓
CreateOrder() GET returns form
  ↓
Customer fills multi-step form (6 steps)
  ↓
Customer uploads images, enters measurements, selects date
  ↓
CreateOrder() POST validates all inputs
  ↓
OrderService.CreateOrderAsync() called
  ↓
Images uploaded to file storage via FileUploadService
  ↓
Order entity created with status "QuotePending"
  ↓
Notification sent to Tailor
  ↓
Customer redirected to OrderDetails page
  ↓
Customer sees order with real-time status tracking
```

**Owner:** Eriny

---

## PHASE 1.2: Controller Implementation

### Detailed Implementation: OrdersController.cs

**File:** `Controllers/OrdersController.cs`

**Purpose:** Handle all HTTP requests related to order operations

---

#### Step 1: Class Declaration and Dependency Injection

**Code:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TafsilkPlatform.Models;
using TafsilkPlatform.Services;
using TafsilkPlatform.Data;
using TafsilkPlatform.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            IFileUploadService fileUploadService,
            ILogger<OrdersController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Action methods follow...
    }
}
```

**Explanation:**
- Dependency Injection: Constructor receives services that will be used throughout controller
- IUnitOfWork: Provides access to all repositories
- IOrderService: Business logic for order operations
- IFileUploadService: Handles image uploads to cloud storage
- ILogger: For debugging and error tracking

---

#### Step 2: CreateOrder() - GET Action (Display Form)

**Detailed Code with Explanations:**

```csharp
/// <summary>
/// Displays the order creation form (GET request).
/// Allows customer to initiate new order with optional pre-selected tailor.
/// </summary>
/// <param name="tailorId">Optional: Pre-fill form if coming from tailor profile</param>
/// <returns>Order creation form view</returns>
[HttpGet]
[Route("create")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CreateOrder(Guid? tailorId)
{
    try
    {
        _logger.LogInformation($"Customer {User.FindFirst(ClaimTypes.NameIdentifier)?.Value} accessing order creation form");

        TailorProfile? tailor = null;

        // If tailorId provided, fetch tailor details to pre-fill form
        if (tailorId.HasValue)
        {
            tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId.Value);

            if (tailor == null)
            {
                _logger.LogWarning($"Tailor {tailorId} not found");
                ModelState.AddModelError("TailorId", "Tailor not found");
                // Fallback: allow customer to select any tailor
            }

            if (!tailor.IsVerified)
            {
                _logger.LogWarning($"Tailor {tailorId} is not verified");
                return BadRequest("Selected tailor is not verified and cannot accept orders");
            }
        }

        // Create ViewModel with pre-filled data (if tailor selected)
        var model = new CreateOrderViewModel
        {
            TailorId = tailorId ?? Guid.Empty,
            TailorName = tailor?.ShopName ?? "Select a Tailor",
            ServiceType = null, // Will be filled by customer
            CurrentStep = 1, // Start at step 1
            EstimatedPrice = 0, // Will be calculated
            Images = new List<IFormFile>(),
            Measurements = new MeasurementsDto(),
            Notes = null
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error displaying order form: {ex.Message}");
        return StatusCode(500, "An error occurred while loading the form");
    }
}
```

**Key Points:**
- **Authorization:** Only authenticated customers can access
- **Tailor Pre-selection:** If coming from tailor profile, form is pre-filled
- **Validation:** Check if tailor exists and is verified before accepting
- **Error Handling:** Log all errors and show user-friendly messages
- **ViewModel:** Contains all form data structure

---

#### Step 3: CreateOrder() - POST Action (Process Submission)

**Detailed Implementation:**

```csharp
/// <summary>
/// Processes order creation form submission (POST request).
/// Validates all inputs, uploads images, creates order record.
/// </summary>
/// <param name="request">Order creation request with all form data</param>
/// <returns>Redirect to order details page on success, form on error</returns>
[HttpPost]
[Route("create")]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    try
    {
        // ==================== VALIDATION PHASE ====================

        // Step 1: Server-side validation (client-side validation in form is not enough)
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state in order creation");
            return BadRequest(ModelState);
        }

        // Step 2: Extract current user ID from authentication claims
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var customerGuid))
        {
            _logger.LogError("Unable to identify authenticated customer");
            return Unauthorized("Unable to identify your account");
        }

        // Step 3: Validate tailor exists and is verified
        var tailor = await _unitOfWork.Tailors.GetByIdAsync(request.TailorId);
        if (tailor == null)
        {
            _logger.LogWarning($"Order attempt with non-existent tailor {request.TailorId}");
            ModelState.AddModelError(nameof(request.TailorId), "Selected tailor does not exist");
            return BadRequest(ModelState);
        }

        if (!tailor.IsVerified)
        {
            _logger.LogWarning($"Order attempt with unverified tailor {request.TailorId}");
            return BadRequest("Selected tailor is not verified and cannot accept orders at this time");
        }

        // Step 4: Validate images
        if (request.Images == null || !request.Images.Any())
        {
            ModelState.AddModelError(nameof(request.Images), "At least one image is required");
            return BadRequest(ModelState);
        }

        if (request.Images.Count > 10)
        {
            ModelState.AddModelError(nameof(request.Images), "Maximum 10 images allowed");
            return BadRequest(ModelState);
        }

        // ==================== BUSINESS LOGIC PHASE ====================

        _logger.LogInformation($"Creating order for customer {customerGuid} with tailor {request.TailorId}");

        // Step 5: Call OrderService to handle order creation
        // OrderService will:
        //   - Upload images to cloud storage
        //   - Create Order record in database
        //   - Initialize order status as "QuotePending"
        //   - Send notification to tailor
        var (success, errorMessage, orderId) = await _orderService.CreateOrderAsync(
            customerGuid,
            request
        );

        if (!success || !orderId.HasValue)
        {
            _logger.LogError($"Order creation failed: {errorMessage}");
            ModelState.AddModelError("", errorMessage ?? "Error creating order");
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Order created successfully: {orderId}");

        // ==================== RESPONSE PHASE ====================

        // Step 6: Show success message and redirect to order tracking page
        TempData["SuccessMessage"] = "Order created successfully! The tailor will respond within 1 hour.";
        return RedirectToAction(nameof(OrderDetails), new { orderId = orderId.Value });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception in CreateOrder POST: {ex.Message}\n{ex.StackTrace}");
        ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
        return View(request);
    }
}
```

**Critical Implementation Details:**

**1. Model State Validation**
```csharp
// Check ModelState BEFORE processing
if (!ModelState.IsValid)
{
    // Log validation errors for debugging
    foreach (var state in ModelState.Values)
    {
        foreach (var error in state.Errors)
        {
            _logger.LogWarning($"Validation error: {error.ErrorMessage}");
        }
    }
    return BadRequest(ModelState);
}
```

**2. User Identity Extraction**
```csharp
// Best practice: Always extract and validate user ID
var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (!Guid.TryParse(customerId, out var customerGuid))
{
    return Unauthorized(); // Don't leak information about auth failure
}
```

**3. Tailor Verification Check**
```csharp
// Prevent orders to unverified tailors
var tailor = await _unitOfWork.Tailors.GetByIdAsync(request.TailorId);
if (!tailor?.IsVerified ?? true) // Use null-coalescing for safety
{
    return BadRequest("Tailor not verified");
}
```

**4. Service Layer Delegation**
```csharp
// Don't do business logic in controller
// Delegate to service: CreateOrderAsync handles:
// - Image uploads
// - Order creation
// - Database saves
// - Notifications
var (success, error, orderId) = await _orderService.CreateOrderAsync(customerGuid, request);
```

---

#### Step 4: OrderDetails() Action (GET)

**Code with Detailed Explanations:**

```csharp
/// <summary>
/// Displays complete order details with real-time tracking timeline.
/// Accessible by order customer, assigned tailor, or admin.
/// </summary>
[HttpGet]
[Route("details/{orderId:guid}")]
[Authorize]
public async Task<IActionResult> OrderDetails(Guid orderId)
{
    try
    {
        _logger.LogInformation($"Fetching order details for {orderId}");

        // ==================== AUTHORIZATION PHASE ====================

        // Step 1: Fetch complete order with all related data (eager loading)
        var order = await _unitOfWork.Orders.GetOrderWithAllDetailsAsync(orderId);

        if (order == null)
        {
            _logger.LogWarning($"Order {orderId} not found");
            return NotFound($"Order {orderId} does not exist");
        }

        // Step 2: Extract current user ID
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(currentUserId, out var userId))
        {
            return Unauthorized("Unable to identify user");
        }

        // Step 3: Verify authorization - only customer, tailor, or admin can view
        bool isCustomer = order.CustomerId == userId;
        bool isTailor = order.TailorId == userId;
        bool isAdmin = User.IsInRole("Admin");

        if (!isCustomer && !isTailor && !isAdmin)
        {
            _logger.LogWarning($"Unauthorized access attempt to order {orderId} by user {userId}");
            return Forbid("You don't have permission to view this order");
        }

        // ==================== DATA PREPARATION PHASE ====================

        // Step 4: Prepare ViewModel with all relevant data
        var model = new OrderDetailsViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.Id.ToString().Substring(0, 8).ToUpper(),

            // Customer Information
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? "Customer",
            CustomerPhone = order.Customer?.PhoneNumber,

            // Tailor Information
            TailorId = order.TailorId,
            TailorName = order.Tailor?.ShopName ?? "Tailor",
            TailorRating = order.Tailor?.AverageRating ?? 0,
            TailorLocation = order.Tailor?.Address,

            // Order Details
            ServiceType = order.OrderType.ToString(),
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            DepositPaid = order.DepositPaid,

            // Dates
            CreatedDate = order.CreatedAt,
            ScheduledDate = order.DueDate,

            // Related Data
            OrderImages = order.OrderImages?.ToList() ?? new(),
            StatusHistory = order.StatusHistory
                ?.OrderByDescending(h => h.ChangedAt)
                .ToList() ?? new(),
            Messages = order.OrderMessages
                ?.OrderBy(m => m.SentAt)
                .ToList() ?? new(),

            // Permission Flags
            IsCustomer = isCustomer,
            IsTailor = isTailor,
            CanUpdateStatus = isTailor && order.Status != OrderStatus.Completed,
            CanCancelOrder = isCustomer && 
                (order.Status == OrderStatus.QuotePending || 
                 order.Status == OrderStatus.QuoteReceived),
            CanMessagePartner = isCustomer || isTailor
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error fetching order details: {ex.Message}");
        return StatusCode(500, "An error occurred while loading order details");
    }
}
```

**Key Architectural Decisions:**

**1. Eager Loading Pattern**
```csharp
// Use custom repository method for single optimized query
var order = await _unitOfWork.Orders.GetOrderWithAllDetailsAsync(orderId);

// This method should include:
return await _set
    .Include(o => o.Customer)
    .Include(o => o.Tailor)
    .Include(o => o.OrderImages)
    .Include(o => o.StatusHistory)
    .Include(o => o.OrderMessages)
    .FirstOrDefaultAsync(o => o.Id == orderId);
```

**2. Authorization Pattern**
```csharp
// Check permissions after fetching data (not before)
// Why? If you check before, you leak information about order existence
bool isCustomer = order.CustomerId == userId;
bool isTailor = order.TailorId == userId;
bool isAdmin = User.IsInRole("Admin");

if (!isCustomer && !isTailor && !isAdmin)
    return Forbid();
```

**3. Permission Flags for View**
```csharp
// Pass flags to view so it can show/hide buttons
CanUpdateStatus = isTailor && order.Status != OrderStatus.Completed,
CanCancelOrder = isCustomer && /* valid statuses */,
CanMessagePartner = isCustomer || isTailor
```

---

#### Step 5: UpdateOrderStatus() Action (POST)

**Implementation for Tailor Status Updates:**

```csharp
/// <summary>
/// Tailor updates order status (e.g., In Progress, Quality Check, Ready for Pickup).
/// Validates state transitions and sends notifications to customer.
/// </summary>
[HttpPost]
[Route("update-status")]
[Authorize(Roles = "Tailor")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateOrderStatus(
    [FromBody] UpdateOrderStatusRequest request)
{
    try
    {
        _logger.LogInformation($"Status update request: Order {request.OrderId} → {request.NewStatus}");

        // ==================== VALIDATION PHASE ====================

        if (request.OrderId == Guid.Empty)
            return BadRequest("Invalid order ID");

        // Fetch order
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
            return NotFound("Order not found");

        // Verify tailor
        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        if (order.TailorId != tailorGuid)
        {
            _logger.LogWarning($"Unauthorized status update attempt: Tailor {tailorGuid} for order {request.OrderId}");
            return Forbid("You are not the tailor assigned to this order");
        }

        // ==================== BUSINESS LOGIC PHASE ====================

        // Validate status transition (business rules)
        var isValidTransition = IsValidStatusTransition(order.Status, request.NewStatus);
        if (!isValidTransition)
        {
            _logger.LogWarning($"Invalid status transition: {order.Status} → {request.NewStatus}");
            return BadRequest(
                $"Cannot transition from {order.Status} to {request.NewStatus}. " +
                "Status transitions must follow the workflow."
            );
        }

        // Call service to update status and create history record
        var (success, errorMessage) = await _orderService.UpdateOrderStatusAsync(
            request.OrderId,
            request.NewStatus
        );

        if (!success)
        {
            _logger.LogError($"Service error updating status: {errorMessage}");
            return BadRequest(errorMessage);
        }

        // ==================== RESPONSE PHASE ====================

        _logger.LogInformation($"Order status updated successfully: {order.Status} → {request.NewStatus}");

        return Json(new
        {
            success = true,
            message = $"Order status updated to {request.NewStatus}",
            newStatus = request.NewStatus.ToString(),
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception in UpdateOrderStatus: {ex.Message}");
        return StatusCode(500, $"Error updating status: {ex.Message}");
    }
}

/// <summary>
/// Validates if status transition is allowed by business rules.
/// </summary>
private bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
{
    // Define allowed state transitions
    var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
    {
        {
            OrderStatus.QuotePending,
            new List<OrderStatus> { OrderStatus.QuoteReceived, OrderStatus.Cancelled }
        },
        {
            OrderStatus.QuoteReceived,
            new List<OrderStatus> { OrderStatus.OrderConfirmed, OrderStatus.Cancelled }
        },
        {
            OrderStatus.OrderConfirmed,
            new List<OrderStatus> { OrderStatus.InProgress }
        },
        {
            OrderStatus.InProgress,
            new List<OrderStatus> { OrderStatus.QualityCheck, OrderStatus.InProgress }
        },
        {
            OrderStatus.QualityCheck,
            new List<OrderStatus> { OrderStatus.ReadyForPickup, OrderStatus.InProgress }
        },
        {
            OrderStatus.ReadyForPickup,
            new List<OrderStatus> { OrderStatus.Completed }
        },
        {
            OrderStatus.Completed,
            new List<OrderStatus>() // Terminal state - no transitions allowed
        }
    };

    return validTransitions.ContainsKey(from) && validTransitions[from].Contains(to);
}
```

**Status Transition Rules Explained:**

```
QuotePending
├─→ QuoteReceived (tailor provides quote)
└─→ Cancelled (customer cancels)

QuoteReceived
├─→ OrderConfirmed (customer approves quote)
└─→ Cancelled

OrderConfirmed
└─→ InProgress (tailor starts work)

InProgress
├─→ QualityCheck (tailor checks quality before delivery)
└─→ InProgress (can stay in progress, no forced progression)

QualityCheck
├─→ ReadyForPickup (quality approved)
└─→ InProgress (need to fix issues)

ReadyForPickup
└─→ Completed (customer receives and pays)

Completed (Terminal - no more changes)
```

---

#### Step 6: CustomerOrders() Action

```csharp
/// <summary>
/// Displays paginated list of customer's orders with filtering options.
/// Shows active, completed, or cancelled orders.
/// </summary>
[HttpGet]
[Route("my-orders")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CustomerOrders(string? filter = "active", int page = 1)
{
    try
    {
        _logger.LogInformation($"Fetching customer orders: filter={filter}, page={page}");

        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        // ==================== QUERY PHASE ====================

        // Fetch orders based on filter
        IEnumerable<Order> orders = filter?.ToLower() switch
        {
            "active" => await _unitOfWork.Orders.GetCustomerActiveOrdersAsync(customerGuid),
            "completed" => await _unitOfWork.Orders.GetCustomerCompletedOrdersAsync(customerGuid),
            "cancelled" => await _unitOfWork.Orders.GetCustomerCancelledOrdersAsync(customerGuid),
            _ => await _unitOfWork.Orders.GetCustomerAllOrdersAsync(customerGuid)
        };

        // ==================== PAGINATION PHASE ====================

        const int pageSize = 10; // 10 orders per page

        // Calculate pagination metrics
        var totalCount = orders.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Validate page number
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        // Get page of orders
        var paginatedOrders = orders
            .OrderByDescending(o => o.CreatedAt) // Most recent first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ==================== VIEW MODEL CONSTRUCTION ====================

        // Convert to DTOs for display
        var orderSummaries = paginatedOrders.Select(o => new OrderSummaryDto
        {
            OrderId = o.Id,
            OrderNumber = o.Id.ToString().Substring(0, 8).ToUpper(),
            TailorName = o.Tailor?.ShopName ?? "Unknown Tailor",
            TailorRating = o.Tailor?.AverageRating ?? 0,
            ServiceType = o.OrderType.ToString(),
            Status = o.Status,
            StatusDisplayName = GetStatusDisplayName(o.Status),
            Price = o.TotalPrice,
            DepositPaid = o.DepositPaid,
            CreatedDate = o.CreatedAt,
            ScheduledDate = o.DueDate,
            DaysAgo = (int)(DateTime.UtcNow - o.CreatedAt).TotalDays,
            HasUnreadMessages = o.OrderMessages?
                .Any(m => !m.IsRead && m.SenderId != customerGuid) ?? false
        }).ToList();

        var model = new CustomerOrdersViewModel
        {
            Orders = orderSummaries,
            CurrentFilter = filter ?? "active",
            CurrentPage = page,
            TotalPages = totalPages,
            TotalOrders = totalCount,
            TotalActive = (await _unitOfWork.Orders
                .GetCustomerActiveOrdersAsync(customerGuid)).Count(),
            TotalCompleted = (await _unitOfWork.Orders
                .GetCustomerCompletedOrdersAsync(customerGuid)).Count()
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error fetching customer orders: {ex.Message}");
        return StatusCode(500, "Error loading orders");
    }
}

/// <summary>
/// Converts OrderStatus enum to customer-friendly display name.
/// </summary>
private string GetStatusDisplayName(OrderStatus status)
{
    return status switch
    {
        OrderStatus.QuotePending => "Waiting for Quote",
        OrderStatus.QuoteReceived => "Quote Received",
        OrderStatus.OrderConfirmed => "Confirmed",
        OrderStatus.InProgress => "In Progress",
        OrderStatus.QualityCheck => "Quality Check",
        OrderStatus.ReadyForPickup => "Ready for Pickup",
        OrderStatus.Completed => "Completed",
        OrderStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
}
```

---

#### Step 7: TailorOrders() Action

Similar structure to CustomerOrders() but for tailors with "New" filter for pending orders.

---

## PHASE 1.3: Service Layer Implementation

### OrderService.cs - Core Business Logic

**File Location:** `Services/OrderService.cs`

---

#### Implementation Pattern

```csharp
using Microsoft.Extensions.Logging;
using TafsilkPlatform.Data;
using TafsilkPlatform.Models;
using TafsilkPlatform.ViewModels;

namespace TafsilkPlatform.Services
{
    /// <summary>
    /// Business logic for order operations.
    /// Handles: creation, updates, status changes, notifications.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IFileUploadService fileUploadService,
            INotificationService notificationService,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _notificationService = notificationService;
            _logger = logger;
        }

        // Implementation methods follow...
    }
}
```

---

#### CreateOrderAsync() - Full Implementation

```csharp
/// <summary>
/// Creates a new order with images and sends notification to tailor.
/// Handles image uploads, order creation, and audit logging.
/// </summary>
public async Task<(bool Success, string? ErrorMessage, Guid? OrderId)> CreateOrderAsync(
    Guid customerId,
    CreateOrderRequest request)
{
    using (var transaction = _unitOfWork.BeginTransaction())
    {
        try
        {
            _logger.LogInformation($"[OrderService] Creating order for customer {customerId}");

            // ==================== VALIDATION ====================

            // Verify tailor exists and is verified
            var tailor = await _unitOfWork.Tailors.GetByIdAsync(request.TailorId);
            if (tailor == null)
            {
                const string msg = "Tailor not found";
                _logger.LogWarning($"[OrderService] {msg}");
                return (false, msg, null);
            }

            if (!tailor.IsVerified)
            {
                const string msg = "Tailor is not verified";
                _logger.LogWarning($"[OrderService] {msg}");
                return (false, msg, null);
            }

            // Verify customer exists
            var customer = await _unitOfWork.Users.GetByIdAsync(customerId);
            if (customer == null)
            {
                const string msg = "Customer not found";
                _logger.LogWarning($"[OrderService] {msg}");
                return (false, msg, null);
            }

            // ==================== IMAGE UPLOAD ====================

            var imageUrls = new List<string>();

            if (request.Images != null && request.Images.Any())
            {
                foreach (var image in request.Images)
                {
                    // Validate file
                    if (image.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        const string msg = "Image size exceeds 5MB limit";
                        await transaction.RollbackAsync();
                        return (false, msg, null);
                    }

                    try
                    {
                        // Upload to cloud storage
                        var url = await _fileUploadService.UploadFileAsync(
                            image,
                            $"orders/{customerId}" // Organize by customer
                        );

                        if (string.IsNullOrEmpty(url))
                        {
                            throw new Exception("Upload returned null URL");
                        }

                        imageUrls.Add(url);
                        _logger.LogInformation($"[OrderService] Image uploaded: {url}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[OrderService] Image upload failed: {ex.Message}");
                        await transaction.RollbackAsync();
                        return (false, $"Image upload failed: {ex.Message}", null);
                    }
                }
            }

            // ==================== ORDER CREATION ====================

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TailorId = request.TailorId,
                OrderType = request.ServiceType, // Enum value
                Description = request.Notes,
                CreatedAt = DateTime.UtcNow,
                DueDate = request.ScheduledDate,
                Status = OrderStatus.QuotePending, // Initial status
                TotalPrice = 0, // Tailor will provide in quote
                DepositPaid = false, // Not paid until checkout
                IsActive = true,
                OrderImages = new List<OrderImages>(),
                OrderItems = new List<OrderItem>(),
                StatusHistory = new List<OrderStatusHistory>()
            };

            // ==================== ADD IMAGES TO ORDER ====================

            foreach (var url in imageUrls)
            {
                order.OrderImages.Add(new OrderImages
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ImageUrl = url,
                    UploaderId = customerId,
                    UploadedAt = DateTime.UtcNow,
                    IsBeforeAfter = false,
                    Caption = null
                });
            }

            // ==================== ADD INITIAL STATUS HISTORY ====================

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                OldStatus = null, // No previous status
                NewStatus = OrderStatus.QuotePending,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = customerId,
                Reason = "Order created"
            });

            // ==================== SAVE TO DATABASE ====================

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[OrderService] Order saved: {order.Id}");

            // ==================== SEND NOTIFICATIONS ====================

            try
            {
                // Notify tailor of new order
                await _notificationService.NotifyTailorNewOrderAsync(
                    order.TailorId,
                    order.Id,
                    customer.Name,
                    order.OrderType.ToString()
                );

                _logger.LogInformation($"[OrderService] Tailor notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[OrderService] Notification failed: {ex.Message}");
                // Don't fail the order creation if notification fails
                // (notification can be retried later)
            }

            // ==================== COMMIT TRANSACTION ====================

            await transaction.CommitAsync();

            _logger.LogInformation($"[OrderService] Order creation completed: {order.Id}");

            return (true, null, order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[OrderService] Exception in CreateOrderAsync: {ex.Message}\n{ex.StackTrace}");
            await transaction.RollbackAsync();
            return (false, $"Error creating order: {ex.Message}", null);
        }
    }
}
```

**Critical Patterns Used:**

1. **Transaction Management**
```csharp
using (var transaction = _unitOfWork.BeginTransaction())
{
    try
    {
        // All database operations
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        // If any step fails, everything rolls back
    }
}
```

2. **Structured Logging**
```csharp
_logger.LogInformation($"[OrderService] Creating order for customer {customerId}");
// Always log action start, important steps, and completion
// Use consistent format: [ClassName] message
```

3. **Error Handling Strategy**
```csharp
if (condition)
{
    const string msg = "Meaningful error message";
    _logger.LogWarning($"[OrderService] {msg}");
    return (false, msg, null); // Return tuple with error info
}
```

---

#### UpdateOrderStatusAsync() - Implementation

```csharp
/// <summary>
/// Updates order status and creates history entry.
/// Sends notification to customer about status change.
/// </summary>
public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(
    Guid orderId,
    OrderStatus newStatus)
{
    using (var transaction = _unitOfWork.BeginTransaction())
    {
        try
        {
            _logger.LogInformation($"[OrderService] Updating order {orderId} status to {newStatus}");

            // ==================== FETCH ORDER ====================

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                const string msg = "Order not found";
                _logger.LogWarning(msg);
                return (false, msg);
            }

            var oldStatus = order.Status;

            // ==================== VALIDATE TRANSITION ====================

            if (!IsValidStatusTransition(oldStatus, newStatus))
            {
                const string msg = $"Invalid transition from {oldStatus} to {newStatus}";
                _logger.LogWarning(msg);
                return (false, msg);
            }

            // ==================== UPDATE STATUS ====================

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // ==================== CREATE HISTORY ENTRY ====================

            var history = new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = order.TailorId, // Tailor made the change
                Reason = GenerateStatusChangeReason(oldStatus, newStatus)
            };

            // ==================== SAVE CHANGES ====================

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[OrderService] Status updated and saved");

            // ==================== SEND NOTIFICATIONS ====================

            await _notificationService.NotifyCustomerOrderStatusAsync(
                order.CustomerId,
                orderId,
                newStatus
            );

            await transaction.CommitAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[OrderService] Exception: {ex.Message}");
            await transaction.RollbackAsync();
            return (false, $"Error: {ex.Message}");
        }
    }
}

private bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
{
    var validTransitions = new Dictionary<OrderStatus, HashSet<OrderStatus>>
    {
        { OrderStatus.QuotePending, new() { OrderStatus.QuoteReceived, OrderStatus.Cancelled } },
        { OrderStatus.QuoteReceived, new() { OrderStatus.OrderConfirmed, OrderStatus.Cancelled } },
        { OrderStatus.OrderConfirmed, new() { OrderStatus.InProgress } },
        { OrderStatus.InProgress, new() { OrderStatus.QualityCheck } },
        { OrderStatus.QualityCheck, new() { OrderStatus.ReadyForPickup, OrderStatus.InProgress } },
        { OrderStatus.ReadyForPickup, new() { OrderStatus.Completed } },
        { OrderStatus.Completed, new() }, // Terminal
        { OrderStatus.Cancelled, new() }  // Terminal
    };

    return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}

private string GenerateStatusChangeReason(OrderStatus from, OrderStatus to)
{
    return (from, to) switch
    {
        (OrderStatus.QuotePending, OrderStatus.QuoteReceived) => "Quote provided by tailor",
        (OrderStatus.QuoteReceived, OrderStatus.OrderConfirmed) => "Quote accepted by customer",
        (OrderStatus.OrderConfirmed, OrderStatus.InProgress) => "Work started",
        (OrderStatus.InProgress, OrderStatus.QualityCheck) => "Quality inspection",
        (OrderStatus.QualityCheck, OrderStatus.ReadyForPickup) => "Ready for delivery/pickup",
        (OrderStatus.ReadyForPickup, OrderStatus.Completed) => "Order completed",
        _ => "Status changed"
    };
}
```

---

#### GetCustomerOrdersAsync() & GetTailorOrdersAsync()

```csharp
public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(
    Guid customerId,
    OrderStatus? filterStatus = null)
{
    var orders = await _unitOfWork.Orders.GetCustomerAllOrdersAsync(customerId);

    if (filterStatus.HasValue)
        orders = orders.Where(o => o.Status == filterStatus.Value);

    return orders.OrderByDescending(o => o.CreatedAt);
}

public async Task<IEnumerable<Order>> GetTailorOrdersAsync(
    Guid tailorId,
    OrderStatus? filterStatus = null)
{
    var orders = await _unitOfWork.Orders.GetTailorAllOrdersAsync(tailorId);

    if (filterStatus.HasValue)
        orders = orders.Where(o => o.Status == filterStatus.Value);

    return orders.OrderByDescending(o => o.CreatedAt);
}
```

---

## PHASE 1.4: View Implementation

### Views/Orders/Create.cshtml - Multi-Step Order Form

**Purpose:** Guide customer through 6-step order creation process with progress indicator

**Key Structure:**

```html
<!-- Progress Indicator (Steps 1-6) -->
<div class="progress-bar">
    <div class="step active" id="step1">1. Service Selection</div>
    <div class="step" id="step2">2. Images</div>
    <div class="step" id="step3">3. Measurements</div>
    <div class="step" id="step4">4. Date & Time</div>
    <div class="step" id="step5">5. Price Review</div>
    <div class="step" id="step6">6. Confirm</div>
</div>

<!-- Step 1: Service Selection -->
<div class="form-step" id="step1-content">
    <!-- Radio buttons for service types -->
    <!-- Display pricing and descriptions -->
    <!-- Next button -->
</div>

<!-- Step 2: Image Upload -->
<div class="form-step hidden" id="step2-content">
    <!-- Drag-and-drop area -->
    <!-- File input -->
    <!-- Image preview grid -->
    <!-- Back/Next buttons -->
</div>

<!-- ... More steps ... -->

<!-- Sticky Summary Sidebar (right side) -->
<aside class="summary-panel">
    <h3>Order Summary</h3>
    <!-- Shows all selections -->
    <!-- Real-time price calculation -->
    <!-- Order total -->
</aside>
```

---

### Views/Orders/Details.cshtml - Order Tracking Page

```html
<!-- Order Header -->
<div class="order-header">
    <h1>Order ORD-12345</h1>
    <div class="tailor-info">
        <img src="@Model.TailorImage" />
        <div>
            <h3>@Model.TailorName</h3>
            <span class="rating">@Model.TailorRating ★</span>
        </div>
    </div>
</div>

<!-- Status Timeline -->
<div class="timeline">
    <!-- Shows each status as a step -->
    <!-- Completed steps in green, current in blue, future in gray -->
    <!-- Timeline includes timestamps -->
</div>

<!-- Order Details Grid -->
<div class="order-details">
    <!-- Service type, dates, price -->
    <!-- Images gallery -->
    <!-- Measurements entered -->
</div>

<!-- Messaging Section -->
<section class="messaging">
    <h3>Chat with @Model.TailorName</h3>
    <!-- Message thread -->
    <!-- Message input box -->
</section>

<!-- Actions (based on permissions) -->
<div class="actions">
    @if (Model.CanUpdateStatus)
    {
        <button>Update Status</button>
    }
    @if (Model.CanCancelOrder)
    {
        <button>Cancel Order</button>
    }
</div>
```

---

### Views/Orders/CustomerIndex.cshtml - Order History

```html
<!-- Filter Tabs -->
<div class="filter-tabs">
    <button class="tab active" data-filter="active">Active (3)</button>
    <button class="tab" data-filter="completed">Completed (12)</button>
    <button class="tab" data-filter="cancelled">Cancelled (1)</button>
    <button class="tab" data-filter="all">All (16)</button>
</div>

<!-- Orders List/Grid -->
<div class="orders-grid">
    @foreach (var order in Model.Orders)
    {
        <div class="order-card">
            <h3>@order.OrderNumber</h3>
            <div class="tailor">@order.TailorName ⭐ @order.TailorRating</div>
            <div class="service">@order.ServiceType</div>
            <div class="status">
                <span class="badge" style="background-color: @GetStatusColor(order.Status)">
                    @order.StatusDisplayName
                </span>
            </div>
            <div class="price">@order.Price.ToString("C")</div>
            <button onclick="location.href='/orders/details/@order.OrderId'">View Details</button>
        </div>
    }
</div>

<!-- Pagination -->
<div class="pagination">
    @for (int i = 1; i <= Model.TotalPages; i++)
    {
        <a href="?page=@i" class="page @(i == Model.CurrentPage ? "active" : "")">@i</a>
    }
</div>
```

---

## PHASE 1.5: Repository Methods & Database Queries

### OrderRepository.cs - Add Query Methods

```csharp
public async Task<Order?> GetOrderWithAllDetailsAsync(Guid orderId)
{
    return await _set
        .Include(o => o.Customer)
        .Include(o => o.Tailor)
        .Include(o => o.OrderImages)
        .Include(o => o.StatusHistory)
        .Include(o => o.OrderMessages)
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.Id == orderId);
}

public async Task<IEnumerable<Order>> GetCustomerActiveOrdersAsync(Guid customerId)
{
    var activeStatuses = new[]
    {
        OrderStatus.QuotePending,
        OrderStatus.QuoteReceived,
        OrderStatus.OrderConfirmed,
        OrderStatus.InProgress,
        OrderStatus.QualityCheck,
        OrderStatus.ReadyForPickup
    };

    return await _set
        .Where(o => o.CustomerId == customerId && activeStatuses.Contains(o.Status))
        .Include(o => o.Tailor)
        .Include(o => o.OrderMessages)
        .ToListAsync();
}

public async Task<IEnumerable<Order>> GetCustomerCompletedOrdersAsync(Guid customerId)
{
    return await _set
        .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Completed)
        .Include(o => o.Tailor)
        .ToListAsync();
}

public async Task<IEnumerable<Order>> GetCustomerAllOrdersAsync(Guid customerId)
{
    return await _set
        .Where(o => o.CustomerId == customerId)
        .Include(o => o.Tailor)
        .Include(o => o.OrderMessages)
        .ToListAsync();
}

public async Task<IEnumerable<Order>> GetTailorNewOrdersAsync(Guid tailorId)
{
    return await _set
        .Where(o => o.TailorId == tailorId && o.Status == OrderStatus.QuotePending)
        .Include(o => o.Customer)
        .ToListAsync();
}

// Similar methods for active, completed, all...
```

---

## PHASE 1.6: Program.cs Registration

```csharp
// In Program.cs, add to services configuration:

// Register OrderService
builder.Services.AddScoped<IOrderService, OrderService>();

// Register OrderRepository if using Generic Repository pattern
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Ensure IUnitOfWork is registered
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

---

## PHASE 1.7: Testing & Validation

### Integration Tests

```csharp
[TestFixture]
public class OrderControllerTests
{
    private OrdersController _controller;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IOrderService> _mockOrderService;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(
            _mockUnitOfWork.Object,
            _mockOrderService.Object,
            Mock.Of<IFileUploadService>(),
            Mock.Of<ILogger<OrdersController>>()
        );
    }

    [Test]
    public async Task CreateOrder_WithValidRequest_ReturnsOrderId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            TailorId = Guid.NewGuid(),
            ServiceType = OrderType.Alterations,
            ScheduledDate = DateTime.UtcNow.AddDays(3)
        };

        _mockOrderService
            .Setup(s => s.CreateOrderAsync(It.IsAny<Guid>(), It.IsAny<CreateOrderRequest>()))
            .ReturnsAsync((true, null, Guid.NewGuid()));

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsInstanceOf<RedirectToActionResult>(result);
        _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<Guid>(), request), Times.Once);
    }

    // More tests...
}
```

---

## PHASE 1.8: Code Review Checklist

Before submitting for review:

- [ ] All action methods have XML documentation comments
- [ ] Error handling with try-catch blocks
- [ ] Logging at appropriate levels (Info, Warning, Error)
- [ ] Authorization checks on all protected endpoints
- [ ] CSRF token validation on POST/PUT/DELETE
- [ ] Input validation on server side
- [ ] Database queries use Include() for eager loading
- [ ] No SQL injection vulnerabilities
- [ ] Proper use of async/await
- [ ] Views have proper HTML structure
- [ ] CSS classes follow naming conventions
- [ ] JavaScript events properly bound
- [ ] Pagination works correctly
- [ ] Responsive design on mobile

---

## TASK 1 SUMMARY

**Deliverables:**
OrdersController with 6 actions
OrderService with business logic
4+ Razor views for order workflow
Complete repository methods
ViewModels for data binding
Integration tests
Documentation

**Files Created:** 15-20

---

# TASK 2: REVIEW & RATING SYSTEM (ERINY'S RESPONSIBILITY)

## Overview

The Review & Rating System enables customers to provide feedback on tailors after order completion. This includes multi-dimensional ratings (Quality, Communication, Timeliness, Pricing), photo uploads for evidence, and portfolio management for tailors.

**Why This Task After Task 1?**
- Depends on completed Order entities
- Can start once Order schema is finalized
- Reviews should only be submitted for completed orders

**Success Criteria:**
- Customers can submit detailed reviews with photos
- Ratings are calculated and displayed accurately
- Tailors can manage portfolio images
- Rating distribution analytics available

---

## PHASE 2.1: Planning & Design

### Entity Relationship Mapping

**Entities Involved:**
```
Review (Parent)
├── OrderId (FK) → Order
├── CustomerId (FK) → User
├── TailorId (FK) → User/TailorProfile
├── RatingDimensions[] (Individual dimension ratings)
├── ReviewPhotos[] (Customer-uploaded photos)
└── CreatedAt, UpdatedAt (Timestamps)

PortfolioImage (Parent)
├── TailorId (FK) → User/TailorProfile
└── ImageUrl, Caption, UploadedAt
```

---

### HTTP Route Planning

**Routes to Implement:**
```
GET    /reviews/create/{orderId}           → Review submission form
POST   /reviews/create                     → Process review submission
GET    /reviews/tailor/{tailorId}          → Display all reviews
GET    /portfolio/manage                   → Tailor portfolio page
POST   /portfolio/upload                   → Upload portfolio image
DELETE /portfolio/{imageId}                → Delete portfolio image
GET    /reviews/success/{reviewId}         → Confirmation page
```

---

## PHASE 2.2: ReviewsController Implementation

### Key Action Methods

#### 1. CreateReview() - GET

```csharp
[HttpGet]
[Route("create/{orderId:guid}")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> CreateReview(Guid orderId)
{
    try
    {
        // Fetch order and verify completion
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return NotFound("Order not found");

        // Verify order belongs to current customer
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid) || order.CustomerId != customerGuid)
            return Forbid();

        // Verify order is completed
        if (order.Status != OrderStatus.Completed)
            return BadRequest("Order must be completed before review");

        // Check for existing review
        var existingReview = await _unitOfWork.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == orderId);

        if (existingReview != null)
            return RedirectToAction(nameof(EditReview), new { reviewId = existingReview.Id });

        var model = new CreateReviewViewModel
        {
            OrderId = orderId,
            TailorId = order.TailorId,
            TailorName = order.Tailor?.ShopName,
            ServiceType = order.OrderType,
            OrderDate = order.CreatedAt
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error displaying review form: {ex.Message}");
        return StatusCode(500, "An error occurred");
    }
}
```

---

#### 2. CreateReview() - POST

```csharp
[HttpPost]
[Route("create")]
[Authorize(Roles = "Customer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateReview(CreateReviewRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(customerId, out var customerGuid))
            return Unauthorized();

        var result = await _reviewService.SubmitReviewAsync(
            request.OrderId,
            customerGuid,
            request
        );

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        TempData["SuccessMessage"] = "Thank you for your review!";
        return RedirectToAction(nameof(ReviewSuccess), new { reviewId = result.ReviewId });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Exception in CreateReview POST: {ex.Message}");
        ModelState.AddModelError("", $"Error: {ex.Message}");
        return View(request);
    }
}
```

---

#### 3. TailorReviews() - GET

```csharp
[HttpGet]
[Route("tailor/{tailorId:guid}")]
[AllowAnonymous]
public async Task<IActionResult> TailorReviews(Guid tailorId, string sortBy = "helpful", int page = 1)
{
    try
    {
        var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
        if (tailor == null)
            return NotFound("Tailor not found");

        var reviews = await _unitOfWork.Reviews
            .GetTailorReviewsAsync(tailorId);

        // Sort options: helpful, recent, highest, lowest
        reviews = sortBy switch
        {
            "recent" => reviews.OrderByDescending(r => r.CreatedAt),
            "highest" => reviews.OrderByDescending(r => r.Rating),
            "lowest" => reviews.OrderByDescending(r => r.Rating),
            _ => reviews.OrderByDescending(r => r.Rating)
        };

        const int pageSize = 5;
        var totalCount = reviews.Count();
        var paginatedReviews = reviews
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Calculate rating statistics
        var avgRating = await _reviewService.CalculateTailorRatingAsync(tailorId);
        var ratingDistribution = await _reviewService.GetRatingDistributionAsync(tailorId);
        var dimensionRatings = await _reviewService.GetDimensionRatingsAsync(tailorId);

        var model = new TailorReviewsViewModel
        {
            TailorId = tailorId,
            TailorName = tailor.ShopName,
            AverageRating = avgRating,
            TotalReviews = totalCount,
            RatingDistribution = ratingDistribution,
            DimensionRatings = dimensionRatings,
            Reviews = paginatedReviews,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error fetching reviews: {ex.Message}");
        return StatusCode(500, "Error loading reviews");
    }
}
```

---

#### 4. PortfolioManagement() - GET

```csharp
[HttpGet]
[Route("portfolio/manage")]
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> PortfolioManagement()
{
    try
    {
        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorGuid);
        if (tailor == null)
            return NotFound("Tailor profile not found");

        var portfolioImages = await _unitOfWork.Portfolio
            .GetTailorPortfolioAsync(tailorGuid);

        var model = new PortfolioManagementViewModel
        {
            TailorId = tailorGuid,
            TailorName = tailor.ShopName,
            PortfolioImages = portfolioImages.Select(p => new PortfolioImageDto
            {
                ImageId = p.Id,
                ImageUrl = p.ImageUrl,
                IsBeforeAfter = p.IsBeforeAfter,
                Caption = p.Caption,
                UploadedDate = p.UploadedAt
            }).ToList(),
            TotalImages = portfolioImages.Count(),
            MaxImages = 100,
            CanAddMore = portfolioImages.Count() < 100
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading portfolio: {ex.Message}");
        return StatusCode(500, "Error loading portfolio");
    }
}
```

---

#### 5. UploadPortfolioImage() - POST

```csharp
[HttpPost]
[Route("portfolio/upload")]
[Authorize(Roles = "Tailor")]
public async Task<IActionResult> UploadPortfolioImage(IFormFile image, string? caption, bool isBeforeAfter)
{
    try
    {
        if (image == null || image.Length == 0)
            return BadRequest("No file selected");

        if (image.Length > 5 * 1024 * 1024) // 5MB
            return BadRequest("File size exceeds 5MB limit");

        var tailorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(tailorId, out var tailorGuid))
            return Unauthorized();

        var result = await _portfolioService.UploadPortfolioImageAsync(
            tailorGuid,
            image,
            caption,
            isBeforeAfter
        );

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Json(new 
        { 
            success = true, 
            imageId = result.ImageId,
            imageUrl = result.ImageUrl,
            message = "Image uploaded successfully"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error uploading image: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

## PHASE 2.3: ReviewService Implementation

```csharp
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        ILogger<ReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, Guid? ReviewId)> SubmitReviewAsync(
        Guid orderId,
        Guid customerId,
        CreateReviewRequest request)
    {
        try
        {
            _logger.LogInformation($"[ReviewService] Submitting review for order {orderId}");

            // Validate order exists and is completed
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.CustomerId != customerId)
                return (false, "Order not found or unauthorized", null);

            if (order.Status != OrderStatus.Completed)
                return (false, "Order must be completed", null);

            // Check for existing review
            var existingReview = await _unitOfWork.Reviews
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (existingReview != null)
                return (false, "Review already exists for this order", null);

            // Create review
            var review = new Review
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CustomerId = customerId,
                TailorId = order.TailorId,
                Rating = request.OverallRating,
                Comment = request.ReviewText,
                CreatedAt = DateTime.UtcNow,
                IsVerifiedPurchase = true
            };

            // Add dimension ratings
            if (request.DimensionRatings != null)
            {
                foreach (var dimension in request.DimensionRatings)
                {
                    review.RatingDimensions.Add(new RatingDimension
                    {
                        DimensionName = dimension.Key,
                        Score = dimension.Value
                    });
                }
            }

            // Upload review photos
            if (request.Photos != null && request.Photos.Any())
            {
                foreach (var photo in request.Photos)
                {
                    var url = await _fileUploadService.UploadFileAsync(photo, "reviews");
                    review.ReviewPhotos.Add(new ReviewPhoto
                    {
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Update tailor rating
            await UpdateTailorRatingAsync(order.TailorId);

            _logger.LogInformation($"[ReviewService] Review submitted: {review.Id}");
            return (true, null, review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ReviewService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<decimal> CalculateTailorRatingAsync(Guid tailorId)
    {
        var reviews = await _unitOfWork.Reviews
            .Where(r => r.TailorId == tailorId)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return (decimal)reviews.Average(r => r.Rating);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid tailorId)
    {
        var reviews = await _unitOfWork.Reviews
            .Where(r => r.TailorId == tailorId)
            .ToListAsync();

        return new Dictionary<int, int>
        {
            { 5, reviews.Count(r => r.Rating == 5) },
            { 4, reviews.Count(r => r.Rating == 4) },
            { 3, reviews.Count(r => r.Rating == 3) },
            { 2, reviews.Count(r => r.Rating == 2) },
            { 1, reviews.Count(r => r.Rating == 1) }
        };
    }

    public async Task<Dictionary<string, decimal>> GetDimensionRatingsAsync(Guid tailorId)
    {
        var dimensions = await _unitOfWork.RatingDimensions
            .Where(d => d.Review.TailorId == tailorId)
            .GroupBy(d => d.DimensionName)
            .Select(g => new 
            { 
                Name = g.Key, 
                Average = g.Average(d => d.Score) 
            })
            .ToDictionaryAsync(x => x.Name, x => (decimal)x.Average);

        return dimensions;
    }

    private async Task UpdateTailorRatingAsync(Guid tailorId)
    {
        var rating = await CalculateTailorRatingAsync(tailorId);

        var tailor = await _unitOfWork.Tailors.GetByIdAsync(tailorId);
        if (tailor != null)
        {
            tailor.AverageRating = rating;
            tailor.ReviewCount = await _unitOfWork.Reviews
                .CountAsync(r => r.TailorId == tailorId);

            await _unitOfWork.Tailors.UpdateAsync(tailor);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

---

## PHASE 2.4: PortfolioService Implementation

```csharp
public class PortfolioService : IPortfolioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        ILogger<PortfolioService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, Guid? ImageId, string? ImageUrl)> 
        UploadPortfolioImageAsync(
        Guid tailorId,
        IFormFile image,
        string? caption,
        bool isBeforeAfter)
    {
        try
        {
            // Check portfolio limit
            var currentCount = await _unitOfWork.Portfolio
                .CountAsync(p => p.TailorId == tailorId);

            if (currentCount >= 100)
                return (false, "Portfolio image limit reached (100 images)", null, null);

            // Upload file
            var imageUrl = await _fileUploadService.UploadFileAsync(image, "portfolios");

            // Create record
            var portfolioImage = new PortfolioImage
            {
                Id = Guid.NewGuid(),
                TailorId = tailorId,
                ImageUrl = imageUrl,
                IsBeforeAfter = isBeforeAfter,
                Caption = caption,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.Portfolio.AddAsync(portfolioImage);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[PortfolioService] Image uploaded: {portfolioImage.Id}");
            return (true, null, portfolioImage.Id, imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PortfolioService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}", null, null);
        }
    }

    public async Task<bool> DeletePortfolioImageAsync(Guid imageId, Guid tailorId)
    {
        try
        {
            var image = await _unitOfWork.Portfolio.GetByIdAsync(imageId);
            if (image == null || image.TailorId != tailorId)
                return false;

            await _unitOfWork.Portfolio.DeleteAsync(image);
            await _unitOfWork.SaveChangesAsync();

            await _fileUploadService.DeleteFileAsync(image.ImageUrl);

            _logger.LogInformation($"[PortfolioService] Image deleted: {imageId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PortfolioService] Delete error: {ex.Message}");
            return false;
        }
    }
}
```

---

## PHASE 2.5: Review Views

### Views/Reviews/Create.cshtml
- Multi-dimensional rating form (Quality, Communication, Timeliness, Pricing)
- Written review textarea
- Photo upload (before/after comparison)
- Recommendation yes/no toggle
- Real-time summary preview

### Views/Reviews/TailorReviews.cshtml
- Rating distribution (pie or bar chart)
- Average ratings by dimension
- Individual reviews list
- Filter/sort options
- Verified purchase badge
- Helpful/unhelpful voting

### Views/Reviews/PortfolioManagement.cshtml
- Drag-and-drop upload
- Image grid display
- Delete buttons
- Before/after toggle
- Caption editing
- Progress indicator (X/100 images)

---

## PHASE 2.6: Program.cs Registration

```csharp
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
```

---

---

# TASK 3: PAYMENT & WALLET INTEGRATION (OMAR'S SECONDARY RESPONSIBILITY)

## Overview

The Payment & Wallet System handles all financial transactions. It supports multiple payment methods: Cash on Delivery, Digital Wallets (Vodafone Cash, Orange Cash, Etisalat Cash), Credit/Debit Cards, and Bank Transfers.

**Success Criteria:**
- Multiple payment methods supported
- Transaction history tracked
- Wallet balance management
- Secure payment processing
- Receipt generation

---

## PHASE 3.1: PaymentsController Implementation

### Key Action Methods

#### 1. ProcessPayment() - GET

```csharp
[HttpGet]
[Route("process")]
[Authorize]
public async Task<IActionResult> ProcessPayment(Guid orderId)
{
    try
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return NotFound("Order not found");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (order.CustomerId.ToString() != userId && !User.IsInRole("Admin"))
            return Forbid();

        var model = new ProcessPaymentViewModel
        {
            OrderId = orderId,
            OrderNumber = order.Id.ToString().Substring(0, 8),
            Amount = order.TotalPrice,
            DepositAmount = order.TotalPrice * 0.35m,
            RemainingAmount = order.TotalPrice * 0.65m,
            PaymentMethods = new List<PaymentMethodOption>
            {
                new() { Method = "Cash", Display = "Cash on Delivery", Icon = "💵" },
                new() { Method = "Wallet", Display = "Digital Wallet", Icon = "📱" },
                new() { Method = "Card", Display = "Credit/Debit Card", Icon = "💳" },
                new() { Method = "BankTransfer", Display = "Bank Transfer", Icon = "🏦" }
            }
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading payment page: {ex.Message}");
        return StatusCode(500, "Error loading payment page");
    }
}
```

---

#### 2. ProcessPayment() - POST

```csharp
[HttpPost]
[Route("process")]
[Authorize]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ProcessPayment(ProcessPaymentRequest request)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var result = await _paymentService.ProcessPaymentAsync(
            request.OrderId,
            userGuid,
            request
        );

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return RedirectToAction(nameof(PaymentSuccess), new { transactionId = result.TransactionId });
    }
    catch (Exception ex)
    {
        _logger.LogError($"Payment processing error: {ex.Message}");
        return BadRequest($"Payment processing error: {ex.Message}");
    }
}
```

---

#### 3. PaymentSuccess() - GET

```csharp
[HttpGet]
[Route("success")]
public async Task<IActionResult> PaymentSuccess(string transactionId)
{
    try
    {
        var payment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(p => p.TransactionRef == transactionId);

        if (payment == null)
            return NotFound("Payment not found");

        var model = new PaymentSuccessViewModel
        {
            TransactionId = transactionId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentType,
            PaidAt = payment.PaidAt,
            ReceiptUrl = $"/payments/receipt/{transactionId}"
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading success page: {ex.Message}");
        return StatusCode(500, "Error loading page");
    }
}
```

---

#### 4. WalletManagement() - GET

```csharp
[HttpGet]
[Route("wallet")]
[Authorize]
public async Task<IActionResult> WalletManagement()
{
    try
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userGuid);
        if (wallet == null)
            wallet = await _paymentService.InitializeWalletAsync(userGuid);

        var transactions = await _unitOfWork.Payments
            .Where(p => (p.CustomerId == userGuid || p.TailorId == userGuid) && p.PaymentType == "Wallet")
            .OrderByDescending(p => p.PaidAt)
            .Take(20)
            .ToListAsync();

        var model = new WalletManagementViewModel
        {
            WalletBalance = wallet.Balance,
            LastUpdated = wallet.LastUpdated,
            RecentTransactions = transactions.Select(t => new TransactionDto
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Type = t.OrderId.HasValue ? "Order Payment" : "Top-up",
                Date = t.PaidAt ?? DateTime.UtcNow,
                Status = t.PaymentStatus
            }).ToList(),
            TopUpAmounts = new[] { 100m, 250m, 500m, 1000m }
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error loading wallet: {ex.Message}");
        return StatusCode(500, "Error loading wallet");
    }
}
```

---

#### 5. AddToWallet() - POST

```csharp
[HttpPost]
[Route("wallet/topup")]
[Authorize]
public async Task<IActionResult> AddToWallet(AddToWalletRequest request)
{
    try
    {
        if (request.Amount <= 0)
            return BadRequest("Invalid amount");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var result = await _paymentService.TopUpWalletAsync(
            userGuid,
            request.Amount,
            request.PaymentMethod
        );

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return RedirectToAction(nameof(WalletManagement));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error topping up wallet: {ex.Message}");
        return BadRequest($"Error: {ex.Message}");
    }
}
```

---

## PHASE 3.2: Complete PaymentService Implementation

```csharp
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, string? TransactionId)> ProcessPaymentAsync(
        Guid orderId,
        Guid customerId,
        ProcessPaymentRequest request)
    {
        try
        {
            _logger.LogInformation($"[PaymentService] Processing payment for order {orderId}");

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return (false, "Order not found", null);

            if (order.CustomerId != customerId)
                return (false, "Unauthorized", null);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                CustomerId = customerId,
                TailorId = order.TailorId,
                Amount = request.IsFull ? order.TotalPrice : order.TotalPrice * 0.35m,
                PaymentType = request.PaymentMethod,
                PaymentStatus = "Pending",
                TransactionRef = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            // Process based on payment method
            switch (request.PaymentMethod.ToLower())
            {
                case "cash":
                    payment.PaymentStatus = "CODPending";
                    break;

                case "wallet":
                    var walletResult = await ProcessWalletPaymentAsync(payment, customerId);
                    if (!walletResult.Success)
                        return (false, walletResult.ErrorMessage, null);
                    payment.PaymentStatus = "Completed";
                    payment.PaidAt = DateTime.UtcNow;
                    break;

                case "card":
                    var cardResult = await ProcessCardPaymentAsync(payment, request);
                    if (!cardResult.Success)
                        return (false, cardResult.ErrorMessage, null);
                    payment.PaymentStatus = "Completed";
                    payment.PaidAt = DateTime.UtcNow;
                    break;

                case "banktransfer":
                    payment.PaymentStatus = "BankTransferPending";
                    break;
            }

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyPaymentConfirmationAsync(payment);

            _logger.LogInformation($"[PaymentService] Payment processed: {payment.TransactionRef}");
            return (true, null, payment.TransactionRef);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PaymentService] Error: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> HandleCashPaymentAsync(Guid orderId)
    {
        var payment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.PaymentStatus == "CODPending");

        if (payment == null)
            return (false, "Payment record not found");

        payment.PaymentStatus = "Completed";
        payment.PaidAt = DateTime.UtcNow;

        await _unitOfWork.Payments.UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ProcessDigitalWalletAsync(
        Guid orderId,
        string walletType)
    {
        var payment = await _unitOfWork.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

        if (payment == null)
            return (false, "Payment not found");

        // Call external wallet provider API here

        payment.PaymentStatus = "Completed";
        payment.PaidAt = DateTime.UtcNow;

        await _unitOfWork.Payments.UpdateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    public async Task<IEnumerable<Payment>> GetTransactionHistoryAsync(Guid userId)
    {
        return await _unitOfWork.Payments
            .Where(p => p.CustomerId == userId || p.TailorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Wallet> InitializeWalletAsync(Guid userId)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            LastUpdated = DateTime.UtcNow
        };

        await _unitOfWork.Wallets.AddAsync(wallet);
        await _unitOfWork.SaveChangesAsync();

        return wallet;
    }

    private async Task<(bool Success, string? ErrorMessage)> ProcessWalletPaymentAsync(
        Payment payment,
        Guid customerId)
    {
        var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == customerId);
        if (wallet == null)
            return (false, "Wallet not found");

        if (wallet.Balance < payment.Amount)
            return (false, "Insufficient wallet balance");

        wallet.Balance -= payment.Amount;
        wallet.LastUpdated = DateTime.UtcNow;

        await _unitOfWork.Wallets.UpdateAsync(wallet);
        await _unitOfWork.SaveChangesAsync();

        return (true, null);
    }

    private async Task<(bool Success, string? ErrorMessage)> ProcessCardPaymentAsync(
        Payment payment,
        ProcessPaymentRequest request)
    {
        try
        {
            // Call payment gateway API (Stripe, Fawry, etc.)
            // This is a placeholder for integration

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Card payment failed: {ex.Message}");
            return (false, $"Card payment failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> TopUpWalletAsync(
        Guid userId,
        decimal amount,
        string paymentMethod)
    {
        try
        {
            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                wallet = await InitializeWalletAsync(userId);

            // Process payment for top-up

            wallet.Balance += amount;
            wallet.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.Wallets.UpdateAsync(wallet);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
```

---

## PHASE 3.3: Payment Views

### Views/Payments/Process.cshtml
- Order summary
- Payment method selection
- Discount code input
- Terms checkbox
- Submit button

### Views/Payments/Success.cshtml
- Success checkmark
- Transaction ID
- Amount paid
- Payment method
- Order link
- Receipt download

### Views/Payments/Wallet.cshtml
- Current balance
- Top-up buttons
- Transaction history
- Top-up form

### Views/Payments/TransactionHistory.cshtml
- Transaction list
- Filter options
- Status indicators
- Receipt links

---

## PHASE 3.4: Program.cs Registration

```csharp
builder.Services.AddScoped<IPaymentService, PaymentService>();
```

---

# CROSS-CUTTING IMPLEMENTATION

## Database Migrations

```bash
# Generate and apply migrations
dotnet ef migrations add AddOrdersReviewsPayments
dotnet ef database update
```

---

## AppDbContext.cs Updates

Ensure all DbSets are registered:

```csharp
public DbSet<Order> Orders { get; set; }
public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
public DbSet<OrderImages> OrderImages { get; set; }
public DbSet<Review> Reviews { get; set; }
public DbSet<RatingDimension> RatingDimensions { get; set; }
public DbSet<PortfolioImage> PortfolioImages { get; set; }
public DbSet<Payment> Payments { get; set; }
public DbSet<Wallet> Wallets { get; set; }
```

---

## Navigation Menu Updates

**File:** `Views/Shared/_Layout.cshtml`

```html
@if (User.IsInRole("Customer"))
{
    <li><a href="/orders/my-orders">My Orders</a></li>
    <li><a href="/payments/wallet">My Wallet</a></li>
}

@if (User.IsInRole("Tailor"))
{
    <li><a href="/orders/my-orders-tailor">New Orders</a></li>
    <li><a href="/reviews/portfolio-management">Portfolio</a></li>
}
```

---

## Error Handling Middleware

**File:** `Program.cs`

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionFeature.Error, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "An error occurred",
                status = 500
            });
        }
    });
});
```

---

## Logging Configuration

**File:** `Program.cs`

```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Optional: Use Serilog for file logging
builder.Host.UseSerilog((context, config) =>
    config
        .WriteTo.Console()
        .WriteTo.File(
            "logs/tafsilk-.txt",
            rollingInterval: RollingInterval.Day
        )
        .MinimumLevel.Information()
);
```

---
---

# CROSS-CUTTING IMPLEMENTATION

## Database Migrations

```bash
# Generate and apply migrations
dotnet ef migrations add AddProfilesAdminValidation
dotnet ef database update
```

---

## AppDbContext.cs Updates

Ensure all DbSets are registered:

```csharp
public DbSet<CustomerProfile> Customers { get; set; }
public DbSet<TailorProfile> Tailors { get; set; }
public DbSet<UserAddress> Addresses { get; set; }
public DbSet<TailorService> Services { get; set; }
public DbSet<PortfolioImage> Portfolio { get; set; }
public DbSet<Admin> Admins { get; set; }
public DbSet<AuditLog> AuditLogs { get; set; }
public DbSet<Review> Reviews { get; set; }
public DbSet<Order> Orders { get; set; }
public DbSet<Payment> Payments { get; set; }
public DbSet<Wallet> Wallets { get; set; }
```

---

## Navigation Menu Updates

**File:** `Views/Shared/_Layout.cshtml`

```html
@if (User.IsInRole("Customer"))
{
    <li><a href="/profile/customer">My Profile</a></li>
    <li><a href="/profile/addresses">My Addresses</a></li>
    <li><a href="/profile/favorite-tailors">Saved Tailors</a></li>
}

@if (User.IsInRole("Tailor"))
{
    <li><a href="/profile/tailor">My Profile</a></li>
    <li><a href="/profile/tailor/services">My Services</a></li>
    <li><a href="/profile/tailor/availability">Availability</a></li>
}

@if (User.IsInRole("Admin,SuperAdmin"))
{
    <li><a href="/admin/dashboard">Dashboard</a></li>
    <li><a href="/admin/users">Users</a></li>
    <li><a href="/admin/tailors/verification">Verification</a></li>
}
```

---

## Error Handling Middleware

**File:** `Program.cs`

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionFeature.Error, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "An error occurred",
                status = 500
            });
        }
    });
});
```

---

## Logging Configuration

**File:** `Program.cs`

```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
```

---

---

## CONCLUSION

This comprehensive document provides a complete, step-by-step guide for implementing the Tafsilk Platform ASP.NET MVC website. The new Task 0 (Customer & Tailor Profiles + Admin Dashboard) sets the foundation for all other features.

**Task Breakdown:**
- **Task 0 (Ahmed):** Profiles, Portfolio Showcase, Admin Dashboard, Validation
- **Task 1 (Eriny):** Order Management System
- **Task 2 (Eriny):** Review & Rating System
- **Task 3 (Omar):** Payment & Wallet Integration


Follow this document as a living guide, updating it as requirements change.
