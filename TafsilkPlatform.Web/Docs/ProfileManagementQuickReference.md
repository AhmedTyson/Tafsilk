# Profile Management - Quick Reference Guide

## 🚀 Quick Navigation URLs

### **Customer Actions**
| Feature | URL | Auth Required |
|---------|-----|---------------|
| View Profile | `/profile/customer` | Customer |
| Manage Addresses | `/profile/addresses` | Customer |
| Add Address | `/profile/addresses/add` | Customer |
| Edit Address | `/profile/addresses/edit/{id}` | Customer |
| Edit Settings | `/UserSettings/Edit` | Customer |

### **Tailor Actions**
| Feature | URL | Auth Required |
|---------|-----|---------------|
| View Profile | `/profile/tailor` | Tailor |
| Manage Services | `/profile/tailor/services` | Tailor |
| Add Service | `/profile/tailor/services/add` | Tailor |
| Edit Service | `/profile/tailor/services/edit/{id}` | Tailor |
| Edit Settings | `/UserSettings/Edit` | Tailor |

### **Public Actions**
| Feature | URL | Auth Required |
|---------|-----|---------------|
| Search Tailors | `/profile/search-tailors` | None |
| Search by City | `/profile/search-tailors?city=الرياض` | None |
| Search by Service | `/profile/search-tailors?serviceType=تفصيل` | None |
| View Tailor Profile | `/profile/tailor/{id}` | None |

---

## 🎯 Adding Navigation Links

### **1. Add to Customer Dashboard**

In `Views/Dashboards/Customer.cshtml`, add:

```html
<div class="dashboard-card">
    <a asp-controller="Profiles" asp-action="CustomerProfile" class="dashboard-link">
        <i class="fas fa-user fa-3x mb-3"></i>
 <h5>الملف الشخصي</h5>
        <p>عرض وتعديل معلوماتك الشخصية</p>
    </a>
</div>

<div class="dashboard-card">
    <a asp-controller="Profiles" asp-action="ManageAddresses" class="dashboard-link">
        <i class="fas fa-map-marker-alt fa-3x mb-3"></i>
        <h5>عناوين التوصيل</h5>
        <p>إدارة عناوين التوصيل الخاصة بك</p>
    </a>
</div>

<div class="dashboard-card">
    <a asp-controller="Profiles" asp-action="SearchTailors" class="dashboard-link">
        <i class="fas fa-search fa-3x mb-3"></i>
        <h5>البحث عن خياطين</h5>
        <p>اكتشف خياطين محترفين بالقرب منك</p>
    </a>
</div>
```

### **2. Add to Tailor Dashboard**

In `Views/Dashboards/Tailor.cshtml`, add:

```html
<div class="dashboard-card">
    <a asp-controller="Profiles" asp-action="TailorProfile" class="dashboard-link">
     <i class="fas fa-store fa-3x mb-3"></i>
        <h5>الملف الشخصي</h5>
  <p>عرض ملفك الشخصي العام</p>
    </a>
</div>

<div class="dashboard-card">
    <a asp-controller="Profiles" asp-action="ManageServices" class="dashboard-link">
        <i class="fas fa-concierge-bell fa-3x mb-3"></i>
        <h5>إدارة الخدمات</h5>
        <p>أضف وعدل خدماتك المتاحة</p>
    </a>
</div>
```

### **3. Add to Unified Navigation**

In `Views/Shared/_UnifiedNav.cshtml`, add to authenticated user menu:

```html
@if (User.IsInRole("Customer"))
{
    <a asp-controller="Profiles" asp-action="ManageAddresses" class="dropdown-item">
   <i class="fas fa-map-marker-alt me-2"></i>عناوين التوصيل
    </a>
}

@if (User.IsInRole("Tailor"))
{
    <a asp-controller="Profiles" asp-action="ManageServices" class="dropdown-item">
    <i class="fas fa-concierge-bell me-2"></i>إدارة الخدمات
</a>
}

<!-- For all users -->
<a asp-controller="Profiles" asp-action="SearchTailors" class="dropdown-item">
    <i class="fas fa-search me-2"></i>البحث عن خياطين
</a>
```

### **4. Add to Homepage**

In `Views/Home/Index.cshtml`, add a prominent search section:

```html
<section class="search-section py-5">
    <div class="container">
  <div class="row">
            <div class="col-lg-8 mx-auto text-center">
    <h2 class="mb-4">ابحث عن أفضل الخياطين</h2>
       <p class="lead text-muted mb-4">اكتشف خياطين محترفين وموثوقين بالقرب منك</p>
       
         <form asp-controller="Profiles" asp-action="SearchTailors" method="get" class="search-form">
   <div class="row g-3">
            <div class="col-md-5">
       <input type="text" name="city" class="form-control form-control-lg" 
         placeholder="المدينة" />
   </div>
          <div class="col-md-5">
                <input type="text" name="serviceType" class="form-control form-control-lg" 
         placeholder="نوع الخدمة" />
         </div>
              <div class="col-md-2">
               <button type="submit" class="btn btn-primary btn-lg w-100">
             <i class="fas fa-search"></i>
     </button>
    </div>
           </div>
       </form>
     
          <div class="mt-3">
  <a asp-controller="Profiles" asp-action="SearchTailors" class="text-muted">
         أو تصفح جميع الخياطين <i class="fas fa-arrow-left ms-1"></i>
            </a>
  </div>
       </div>
        </div>
    </div>
</section>
```

---

## 🔗 Linking Services

### **From Search Results to Public Profile**

Already implemented in `SearchTailors.cshtml`:
```html
<a asp-action="ViewPublicTailorProfile" asp-route-id="@tailor.Id" class="btn btn-primary">
  <i class="fas fa-eye me-2"></i>عرض الملف الشخصي
</a>
```

### **From Public Profile to Book Service**

Add booking button (future implementation):
```html
<a asp-controller="Bookings" asp-action="Create" asp-route-tailorId="@Model.Id" 
   class="btn btn-primary">
    <i class="fas fa-calendar-check me-2"></i>حجز موعد
</a>
```

### **From Address Management to Checkout**

When implementing checkout:
```csharp
// In CheckoutController
var defaultAddress = await _db.UserAddresses
    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

if (defaultAddress == null)
{
    // Redirect to address management
    return RedirectToAction("ManageAddresses", "Profiles");
}
```

---

## 📱 Mobile Navigation

Add bottom navigation for mobile users:

```html
<!-- In _Layout.cshtml, before </body> -->
<nav class="mobile-bottom-nav d-lg-none">
    <a asp-controller="Home" asp-action="Index" class="nav-item">
 <i class="fas fa-home"></i>
        <span>الرئيسية</span>
    </a>
    <a asp-controller="Profiles" asp-action="SearchTailors" class="nav-item">
     <i class="fas fa-search"></i>
        <span>بحث</span>
    </a>
    @if (User.IsInRole("Customer"))
  {
        <a asp-controller="Profiles" asp-action="CustomerProfile" class="nav-item">
         <i class="fas fa-user"></i>
     <span>الملف الشخصي</span>
        </a>
    }
 @if (User.IsInRole("Tailor"))
    {
 <a asp-controller="Profiles" asp-action="TailorProfile" class="nav-item">
            <i class="fas fa-store"></i>
            <span>الملف الشخصي</span>
    </a>
    }
</nav>

<style>
.mobile-bottom-nav {
    position: fixed;
    bottom: 0;
 left: 0;
    right: 0;
    background: white;
    box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
    display: flex;
justify-content: space-around;
 padding: 0.5rem 0;
    z-index: 1000;
}

.mobile-bottom-nav .nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-decoration: none;
    color: #6c757d;
 font-size: 0.75rem;
}

.mobile-bottom-nav .nav-item i {
    font-size: 1.25rem;
    margin-bottom: 0.25rem;
}

.mobile-bottom-nav .nav-item:hover,
.mobile-bottom-nav .nav-item.active {
    color: #2c5aa0;
}
</style>
```

---

## 🧪 Testing Routes

### **Using Browser DevTools**

Test all routes in browser console:

```javascript
// Customer routes
window.location.href = '/profile/customer';
window.location.href = '/profile/addresses';
window.location.href = '/profile/addresses/add';

// Tailor routes
window.location.href = '/profile/tailor';
window.location.href = '/profile/tailor/services';
window.location.href = '/profile/tailor/services/add';

// Public routes
window.location.href = '/profile/search-tailors';
window.location.href = '/profile/search-tailors?city=الرياض';
window.location.href = '/profile/search-tailors?serviceType=تفصيل';
```

### **Using Postman/API Testing**

For API testing:

```
GET https://localhost:5001/profile/customer
Headers: Cookie: .AspNetCore.Cookies=[your-auth-cookie]

GET https://localhost:5001/profile/search-tailors?city=الرياض&page=1

POST https://localhost:5001/profile/addresses/add
Headers: Cookie: .AspNetCore.Cookies=[your-auth-cookie]
Body: {
    "Label": "Home",
    "Street": "123 Main St",
    "City": "الرياض",
    "IsDefault": true
}
```

---

## 🔒 Authorization Matrix

| Route | Anonymous | Customer | Tailor | Admin |
|-------|-----------|----------|--------|-------|
| `/profile/customer` | ❌ | ✅ | ❌ | ✅ |
| `/profile/addresses` | ❌ | ✅ | ❌ | ✅ |
| `/profile/tailor` | ❌ | ❌ | ✅ | ✅ |
| `/profile/tailor/services` | ❌ | ❌ | ✅ | ✅ |
| `/profile/search-tailors` | ✅ | ✅ | ✅ | ✅ |
| `/profile/tailor/{id}` | ✅ | ✅ | ✅ | ✅ |

---

## 🎨 Customizing UI

### **Change Primary Color**

Update in CSS:

```css
:root {
    --primary-color: #2c5aa0;  /* Change this */
    --primary-gradient: linear-gradient(135deg, #2c5aa0 0%, #1e3a5f 100%);
}

.btn-primary {
    background: var(--primary-color);
}

.bg-gradient-primary {
    background: var(--primary-gradient);
}
```

### **Add Custom Fonts**

```html
<!-- In _Layout.cshtml <head> -->
<link href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;900&display=swap" rel="stylesheet">

<style>
body {
    font-family: 'Tajawal', sans-serif;
}
</style>
```

---

## 📊 Analytics Integration

Track user interactions:

```javascript
// In site.js or custom analytics file

function trackProfileView(profileType) {
    if (typeof gtag !== 'undefined') {
 gtag('event', 'profile_view', {
 'profile_type': profileType
        });
    }
}

function trackSearch(city, serviceType) {
    if (typeof gtag !== 'undefined') {
        gtag('event', 'search', {
  'search_term': city + ' - ' + serviceType
        });
    }
}

// Usage
document.addEventListener('DOMContentLoaded', function() {
    // Track profile views
    if (window.location.pathname.includes('/profile/customer')) {
        trackProfileView('customer');
 }
    
    // Track searches
    const searchForm = document.querySelector('.search-form');
    if (searchForm) {
    searchForm.addEventListener('submit', function() {
            const city = this.querySelector('[name="city"]').value;
            const serviceType = this.querySelector('[name="serviceType"]').value;
            trackSearch(city, serviceType);
        });
    }
});
```

---

## 🐛 Common Issues & Solutions

### **Issue: Routes not working**
**Solution:** Ensure routing is registered in `Program.cs`:
```csharp
app.MapControllerRoute(
    name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");
```

### **Issue: Authorization failing**
**Solution:** Check user claims in Razor:
```razor
@using System.Security.Claims
@{
  var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
}
<p>User ID: @userId</p>
<p>Role: @role</p>
```

### **Issue: Views not found**
**Solution:** Ensure view files are in correct location:
- `Views/Profiles/ManageAddresses.cshtml`
- `Views/Profiles/AddAddress.cshtml`
- etc.

---

## 📞 Support Commands

```bash
# Check routes
dotnet aspnet-codegenerator controller --help

# Run migrations
dotnet ef database update

# Clear cache
dotnet clean
dotnet build

# Run project
dotnet run --project TafsilkPlatform.Web
```

---

**Last Updated:** 2024-01-09
**Version:** 1.0.0
