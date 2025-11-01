# Unified Navigation & Footer System

## Overview
This document describes the unified navigation and footer system implemented in the Tafsilk platform. The system provides a consistent, responsive UI across all pages with automatic state management for authenticated and unauthenticated users.

---

## 📁 File Structure

```
Views/
└── Shared/
    ├── _Layout.cshtml        # Main layout file
    ├── _UnifiedNav.cshtml       # Unified navigation partial
    └── _UnifiedFooter.cshtml    # Unified footer partial
```

---

## 🎯 Features

### Unified Navigation (`_UnifiedNav.cshtml`)

#### **For Unauthenticated Users:**
- **Brand Logo** - Links to homepage
- **Navigation Links:**
  - الرئيسية (Home)
  - كيف تعمل المنصة (How it Works)
  - الخياطين (Tailors)
  - تواصل معنا (Contact)
- **Action Buttons:**
  - تسجيل دخول (Login)
  - انضم كخياط (Join as Tailor)
- **Language Toggle** - EN/عربية
- **Mobile Menu** - Slide-in drawer

#### **For Authenticated Users:**
- **All above links PLUS:**
- **Role-Specific Dashboard Links:**
  - Admin → لوحة المسؤول
  - Customer → لوحة التحكم
  - Tailor → لوحة التحكم
  - Corporate → لوحة التحكم
- **Notifications Badge** - Shows unread count
- **User Menu Dropdown:**
  - User avatar & name
  - User role
  - الملف الشخصي (Profile)
- الإعدادات (Settings)
  - المساعدة (Help)
  - تسجيل الخروج (Logout)

---

## 🎨 Visual Design

### Navigation Bar
- **Sticky Position** - Always visible at top
- **White Background** - Clean, modern look
- **Gradient Dashboard Button** - Purple gradient for dashboard link
- **Box Shadow** - Subtle elevation
- **Responsive** - Adapts to all screen sizes

### User Menu
- **Avatar Image** - Generated from user name
- **Dropdown Animation** - Smooth fadeInDown
- **Role Badge** - Shows user role
- **Notification Badge** - Red dot with count

### Mobile Experience
- **Hamburger Menu** - 3-bar icon
- **Slide-in Drawer** - From left side
- **Dark Overlay** - Semi-transparent backdrop
- **Touch-Friendly** - Large tap targets
- **Auto-Close** - Closes after navigation

---

## 🔧 Technical Implementation

### How to Use

#### 1. **In _Layout.cshtml**
```razor
<body>
    <!-- Unified Navigation -->
    @await Html.PartialAsync("_UnifiedNav")

    <main role="main">
     @RenderBody()
    </main>

    <!-- Unified Footer -->
    @await Html.PartialAsync("_UnifiedFooter")
</body>
```

#### 2. **In Individual Pages (if using custom layout)**
```razor
@{
Layout = "~/Views/Shared/_Layout.cshtml";  // Uses unified nav/footer
}
```

#### 3. **For Pages WITHOUT Layout (like Admin Dashboard)**
```razor
@{
    Layout = null;  // No layout, custom navigation
}
```

---

## 📱 Responsive Breakpoints

```css
/* Desktop */
@media (min-width: 1024px) {
    - Full navigation menu visible
    - User info visible
    - All action buttons visible
}

/* Tablet & Mobile */
@media (max-width: 1023px) {
    - Hamburger menu visible
    - Slide-in drawer navigation
    - Compact user menu
}

/* Small Mobile */
@media (max-width: 640px) {
    - Hide notifications on very small screens
    - Compact brand logo
}
```

---

## 🎭 User States

### Anonymous User
```html
<div class="nav-actions">
    <a href="/Account/Login" class="btn btn-outline">
        <i class="fas fa-sign-in-alt"></i>
        <span>تسجيل دخول</span>
    </a>
    <a href="/Account/Register" class="btn btn-primary">
     <i class="fas fa-user-plus"></i>
        <span>انضم كخياط</span>
    </a>
    <button class="lang-toggle">EN</button>
</div>
```

### Authenticated User
```html
<div class="nav-actions">
    <!-- Notifications -->
    <button class="nav-icon-btn" id="notificationBtn">
        <i class="fas fa-bell"></i>
        <span class="notification-count">3</span>
    </button>
    
    <!-- User Menu -->
    <div class="user-menu">
        <button class="user-menu-toggle">
            <img src="avatar.jpg" alt="User" />
        <div class="user-info">
  <span class="user-name">أحمد محمد</span>
 <span class="user-role">عميل</span>
</div>
      <i class="fas fa-chevron-down"></i>
        </button>
    </div>
 
    <button class="lang-toggle">EN</button>
</div>
```

---

## 🦶 Footer System

### Two Footer Types

#### **1. Minimal Footer** (for Auth Pages)
- Automatically shown for Login/Register pages
- Simple copyright + quick links
- Compact design

**Auto-Detect:**
```csharp
var isAuthPage = (controller == "Account" && (action == "Login" || action == "Register"));
```

**Manual Override:**
```razor
@{
    ViewData["MinimalFooter"] = true;  // Force minimal footer
}
```

#### **2. Full Footer** (for All Other Pages)
- 4 columns: About, Quick Links, Support, Contact
- Social media links
- Contact information
- Security badges

---

## 🎨 Styling

### CSS Variables Used
```css
:root {
    --primary: #2563eb;
    --primary-hover: #1e40af;
    --gray-light: #f3f4f6;
    --gray-dark: #374151;
    --text-light: #d1d5db;
    --shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}
```

### Key Classes
- `.unified-navbar` - Main navigation container
- `.navbar-brand` - Logo/brand element
- `.nav-links` - Navigation menu items
- `.nav-actions` - Right-side actions
- `.user-menu-dropdown` - User dropdown menu
- `.mobile-menu-toggle` - Hamburger menu
- `.nav-overlay` - Mobile menu backdrop

---

## 🔐 Security Features

### Anti-Forgery Token
All logout forms include CSRF protection:
```razor
<form asp-controller="Account" asp-action="Logout" method="post">
    @Html.AntiForgeryToken()
    <button type="submit">تسجيل الخروج</button>
</form>
```

### User Claims
The navigation reads user information from claims:
```csharp
var fullName = User.FindFirst("FullName")?.Value 
    ?? User.FindFirst(ClaimTypes.Name)?.Value 
    ?? User.Identity?.Name 
    ?? User.FindFirst(ClaimTypes.Email)?.Value 
    ?? "مستخدم";

var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
```

---

## ♿ Accessibility Features

- **ARIA Labels** - All interactive elements labeled
- **Keyboard Navigation** - Full keyboard support
- **Focus States** - Visible focus indicators
- **Screen Reader Support** - Semantic HTML
- **Role Attributes** - Proper ARIA roles

Example:
```html
<button aria-label="فتح القائمة" aria-controls="navLinks" aria-expanded="false">
    <span class="bar"></span>
</button>
```

---

## 🚀 Performance

### Optimizations
- **CSS-only animations** - No JavaScript for transitions
- **Lazy dropdown** - Menu loads on demand
- **Minimal DOM** - Clean HTML structure
- **No external dependencies** - Pure CSS/JS

### Bundle Size
- Navigation CSS: ~10KB
- Navigation JS: ~3KB
- Footer CSS: ~8KB
- Total: ~21KB (minified)

---

## 🐛 Troubleshooting

### Issue: Navigation not showing
**Solution:** Ensure `_UnifiedNav` partial is included in layout:
```razor
@await Html.PartialAsync("_UnifiedNav")
```

### Issue: User menu not working
**Solution:** Check if jQuery and site.js are loaded:
```html
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/js/site.js"></script>
```

### Issue: Mobile menu not sliding in
**Solution:** Ensure body has proper structure:
```html
<body>
    <div class="nav-overlay" id="navOverlay"></div>
    <!-- Navigation here -->
</body>
```

### Issue: Footer showing wrong version
**Solution:** Set ViewData in controller:
```csharp
ViewData["MinimalFooter"] = true;  // For auth pages
```

---

## 📦 Dependencies

- **Font Awesome 6.4.0** - Icons
- **Cairo Font** - Arabic typography
- **Bootstrap 5 (optional)** - Grid system only
- **jQuery** - Event handling

---

## 🔄 Future Enhancements

### Planned Features
- [ ] Real-time notifications
- [ ] Search functionality in topbar
- [ ] User preferences (theme, language)
- [ ] Breadcrumb navigation
- [ ] Multi-language support (i18n)
- [ ] Dark mode toggle
- [ ] Progressive Web App (PWA) manifest

### Customization Options
```csharp
// In _ViewStart.cshtml or controller
ViewData["ShowSearch"] = true;
ViewData["ShowNotifications"] = false;
ViewData["ShowLanguageToggle"] = true;
ViewData["MinimalFooter"] = false;
```

---

## 📚 Related Documentation

- [Authentication System](./Authentication.md)
- [Role Management](./Roles.md)
- [Responsive Design Guide](./ResponsiveDesign.md)
- [Accessibility Guidelines](./Accessibility.md)

---

## 📞 Support

For issues or questions about the unified navigation system:
- Create an issue in the project repository
- Contact the development team
- Check the troubleshooting section above

---

## ✅ Checklist for Implementation

- [x] Create `_UnifiedNav.cshtml` partial
- [x] Create `_UnifiedFooter.cshtml` partial
- [x] Update `_Layout.cshtml`
- [x] Test unauthenticated user view
- [x] Test authenticated user view
- [x] Test mobile responsive design
- [x] Test all user roles (Admin, Tailor, Customer, Corporate)
- [x] Verify logout functionality
- [x] Test navigation links
- [x] Test dropdown menus
- [x] Verify accessibility
- [x] Cross-browser testing

---

**Last Updated:** @DateTime.Now.ToString("MMMM dd, yyyy")  
**Version:** 1.0.0  
**Maintained By:** Tafsilk Development Team
