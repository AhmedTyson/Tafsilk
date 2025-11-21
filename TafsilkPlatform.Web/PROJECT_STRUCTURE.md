# 📁 Project Structure - Easy Navigation

## 🎯 Where to Find Things

### Core Application Files
```
Program.cs                    # Main entry point - all configuration here
appsettings.json              # Configuration (don't put secrets here!)
appsettings.Development.json # Development-specific config
```

### Controllers (API & Web Pages)
```
Controllers/
├── HomeController.cs         # Home page
├── AccountController.cs      # Login, register, profile
├── StoreController.cs        # E-commerce (products, cart, checkout)
├── OrdersController.cs       # Order management
└── AdminDashboardController.cs # Admin panel
```

### Business Logic
```
Services/
├── AuthService.cs           # Authentication & authorization
├── StoreService.cs          # E-commerce operations
├── OrderService.cs          # Order processing
└── EmailService.cs          # Email sending
```

### Data Access
```
Repositories/
├── UserRepository.cs        # User data
├── ProductRepository.cs     # Product data
├── OrderRepository.cs       # Order data
└── ShoppingCartRepository.cs # Cart data
```

### Database
```
Data/
├── AppDbContext.cs          # Database context (all tables)
├── UnitOfWork.cs            # Transaction management
└── Seed/                    # Initial data
    ├── AdminSeeder.cs       # Creates admin user
    └── ProductSeeder.cs     # Creates sample products
```

### Models (Database Tables)
```
Models/
├── User.cs                  # Users table
├── Product.cs               # Products table
├── Order.cs                  # Orders table
├── ShoppingCart.cs           # Shopping carts
└── Payment.cs                # Payments
```

### Helpers (Use These!)
```
Helpers/
├── ConfigurationHelper.cs   # Config validation
├── ErrorHelper.cs           # Error messages
├── ValidationHelper.cs      # Input validation
└── ControllerHelper.cs      # Controller utilities
```

### Views (Web Pages)
```
Views/
├── Home/                    # Home page
├── Account/                 # Login, register pages
├── Store/                   # Product pages, cart, checkout
├── Orders/                  # Order pages
└── Shared/                  # Layout, navigation
```

### Security
```
Security/
├── PasswordHasher.cs        # Password hashing
├── TokenService.cs          # JWT tokens
└── AuthorizationHandlers.cs # Permission checks
```

### Middleware (Request Processing)
```
Middleware/
├── GlobalExceptionHandlerMiddleware.cs # Catches all errors
├── SecurityHeadersMiddleware.cs        # Security headers
└── UserStatusMiddleware.cs            # User validation
```

---

## 🔍 Quick Find Guide

**Need to...** | **Look in...**
-------------|---------------
Add a new page | `Controllers/` + `Views/`
Add business logic | `Services/`
Access database | `Repositories/`
Change database structure | `Models/` + run migration
Configure app | `Program.cs` or `appsettings.json`
Handle errors | `Middleware/GlobalExceptionHandlerMiddleware.cs`
Validate input | `Helpers/ValidationHelper.cs`
Get user info | `Helpers/ControllerHelper.cs`

---

## 📝 File Naming Convention

- **Controllers**: `*Controller.cs` (e.g., `StoreController.cs`)
- **Services**: `*Service.cs` (e.g., `StoreService.cs`)
- **Repositories**: `*Repository.cs` (e.g., `ProductRepository.cs`)
- **Models**: Singular name (e.g., `Product.cs`, `Order.cs`)
- **ViewModels**: `*ViewModel.cs` (e.g., `ProductViewModel.cs`)
- **Helpers**: `*Helper.cs` (e.g., `ValidationHelper.cs`)

---

## 🎯 Most Important Files

1. **Program.cs** - Everything starts here
2. **AppDbContext.cs** - Database structure
3. **appsettings.json** - Configuration
4. **README.md** - Setup instructions

---

**Tip**: Use Ctrl+P (VS Code) or Ctrl+Shift+T (Visual Studio) to quickly find files by name!

