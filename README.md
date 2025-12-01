# Tafsilk Platform 🧵

A comprehensive, enterprise-grade tailoring services platform built with ASP.NET Core MVC, connecting customers with professional tailors and providing an integrated e-commerce marketplace for tailoring products and services.

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![License](https://img.shields.io/badge/license-Proprietary-red.svg)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Features by Role](#-features-by-role)
- [API Endpoints](#-api-endpoints)
- [Payment Integration](#-payment-integration)
- [Security](#-security)
- [Deployment](#-deployment)
- [Development Guide](#-development-guide)
- [Testing](#-testing)
- [Contributing](#-contributing)
- [License](#-license)

---

## Overview

**Tafsilk** (derived from the Arabic word for "tailoring") is a full-featured platform that bridges the gap between customers seeking tailoring services and professional tailors offering their expertise. The platform includes:

- **Custom Order Management**: Customers can create detailed tailoring orders with measurements and specifications
- **E-Commerce Store**: Browse and purchase tailoring products (fabrics, accessories, tools)
- **Tailor Marketplace**: Search, filter, and connect with professional tailors
- **Multi-Tailor Checkout**: Automatically assigns store orders to respective product owners
- **Payment Processing**: Integrated Stripe payment with multiple payment methods
- **Admin Dashboard**: Comprehensive platform management and analytics

---
- **📋 Order Management**: Receive, process, and update custom tailoring orders
- **🏷️ Product Listing**: Sell tailoring products through the integrated marketplace
- **💰 Revenue Tracking**: Monitor earnings, commissions, and financial analytics
- **📊 Dashboard Analytics**: Track orders, revenue, customer feedback, and performance metrics
- **⚙️ Service Management**: Define and price various tailoring services
- **📅 Order Timeline**: Visual order status workflow management

### For Administrators 🔐

- **👥 User Management**: Manage customers, tailors, and roles
- **📦 Order Oversight**: Monitor and intervene in platform orders
- **📊 Analytics Dashboard**: Platform-wide statistics, revenue, and user activity
- **🧾 Activity Logs**: Track user actions and system events
- **💸 Commission Management**: Configure platform commission rates
- **🏪 Product Moderation**: Approve, edit, or remove products
- **📧 Communication Tools**: Send notifications and announcements
- **⚙️ System Configuration**: Manage platform settings and features

---

## 🚀 Technology Stack

### Backend
- **Framework**: ASP.NET Core 9.0 MVC
- **Language**: C# 13.0
- **Database**: 
  - SQL Server (Production)
  - SQLite (Development)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity + Google OAuth 2.0
- **Logging**: Serilog with file and console sinks
- **Email**: MailKit + SendGrid integration
- **Payment**: Stripe.NET SDK v50.0
- **Validation**: FluentValidation

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Icons**: Font Awesome 6
- **JavaScript**: Vanilla JS + jQuery
- **CSS**: Custom responsive design with premium aesthetics

### Development Tools
- **IDE**: Visual Studio 2022 / VS Code
- **Version Control**: Git + GitHub
- **Package Manager**: NuGet
- **Build Tool**: .NET CLI / MSBuild

### Cloud & Services
- **Email Service**: SendGrid / Gmail SMTP
- **Payment Gateway**: Stripe
- **OAuth Provider**: Google
- **File Storage**: Local file system (configurable for cloud)

---

## 🏗️ Architecture

### Layered Architecture

The solution follows a clean, layered architecture pattern:

```
┌─────────────────────────────────────────┐
│     Presentation Layer (Web)            │
│  Controllers, Views, ViewModels, UI     │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Business Logic Layer (BLL)         │
│   Services, Domain Logic, Validation    │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│    Data Access Layer (DataAccess)       │
│  DbContext, Repositories, Migrations    │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        Domain Layer (Models)            │
│    Entities, DTOs, ViewModels, Enums    │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│      Utility Layer (Utility)            │
│    Helpers, Extensions, Constants       │
└─────────────────────────────────────────┘
```

### Design Patterns

- **Repository Pattern**: Abstraction over data access logic
- **Unit of Work**: Transaction management across repositories
- **Dependency Injection**: Loose coupling and testability
- **Service Layer**: Encapsulation of business logic
- **MVC Pattern**: Separation of concerns
- **DTO Pattern**: Data transfer between layers
- **Factory Pattern**: Object creation (e.g., DesignTimeDbContextFactory)
- **Middleware Pipeline**: Request/response processing

---

## 📁 Project Structure

```
Tafsilk/
├── README.md                          # This file
├── .gitignore                         # Git ignore rules
│
└── TafsilkPlatform/                   # Solution root
    │
    ├── TafsilkPlatform.Web/           # Main web application project
    │   ├── Areas/                     # Feature areas with independent MVC
    │   │   ├── Admin/                 # Admin-specific controllers & views
    │   │   │   ├── Controllers/       # AdminDashboardController, etc.
    │   │   │   └── Views/             # Admin dashboard views
    │   │   ├── Customer/              # Customer area (reserved for future)
    │   │   └── Tailor/                # Tailor-specific features
    │   │       ├── Controllers/       # TailorManagementController, etc.
    │   │       └── Views/             # Tailor dashboard, portfolio, products
    │   │
    │   ├── Controllers/               # Main application controllers
    │   │   ├── AccountController.cs   # Authentication & registration
    │   │   ├── HomeController.cs      # Landing page
    │   │   ├── TailorsController.cs   # Tailor browsing
    │   │   ├── OrdersController.cs    # Order management
    │   │   ├── PaymentsController.cs  # Payment processing
    │   │   ├── ProfilesController.cs  # User profile management
    │   │   ├── DashboardsController.cs # Customer & Tailor dashboards
    │   │   ├── ApiAuthController.cs   # API authentication (JWT)
    │   │   └── OrdersApiController.cs # RESTful API for orders
    │   │
    │   ├── Services/                  # Application services
    │   │   ├── Base/                  # Base service classes
    │   │   ├── Email/                 # Email service implementation
    │   │   ├── Payment/               # Payment processing services
    │   │   │   ├── PaymentProcessorService.cs  # Stripe integration
    │   │   │   └── IPaymentProcessorService.cs
    │   │   ├── AuthService.cs         # Authentication logic
    │   │   ├── StoreService.cs        # E-commerce store logic
    │   │   ├── OrderService.cs        # Order processing
    │   │   ├── ProfileService.cs      # User profile management
    │   │   ├── PortfolioService.cs    # Tailor portfolio management
    │   │   ├── ProductManagementService.cs # Product CRUD
    │   │   ├── AdminService.cs        # Admin operations
    │   │   ├── ValidationService.cs   # Custom validation logic
    │   │   ├── CacheService.cs        # Caching layer
    │   │   └── FileUploadService.cs   # File handling
    │   │
    │   ├── BLL/                       # Business Logic Layer
    │   │   └── OrderBLL.cs            # Order business rules
    │   │
    │   ├── Middleware/                # Custom middleware
    │   │   ├── ProfileCompletionMiddleware.cs
    │   │   ├── RequestTimingMiddleware.cs
    │   │   └── ExceptionHandlingMiddleware.cs
    │   │
    │   ├── Security/                  # Security utilities
    │   │   ├── JwtTokenService.cs
    │   │   └── PasswordHasher.cs
    │   │
    │   ├── Views/                     # Razor views
    │   │   ├── Home/                  # Landing & home pages
    │   │   ├── Account/               # Login, register, etc.
    │   │   ├── Tailors/               # Tailor browsing & details
    │   │   ├── Orders/                # Order creation & tracking
    │   │   ├── Dashboards/            # User dashboards
    │   │   ├── Profiles/              # Profile management
    │   │   ├── Shared/                # Shared layouts & partials
    │   │   │   ├── _Layout.cshtml
    │   │   │   ├── _Navigation.cshtml
    │   │   │   └── Components/        # View components
    │   │   └── Store/                 # Product store views
    │   │
    │   ├── wwwroot/                   # Static files
    │   │   ├── css/                   # Stylesheets
    │   │   ├── js/                    # JavaScript files
    │   │   ├── images/                # Static images
    │   │   ├── lib/                   # Third-party libraries
    │   │   └── uploads/               # User-uploaded files
    │   │       ├── profiles/          # Profile pictures
    │   │       ├── products/          # Product images
    │   │       └── portfolio/         # Portfolio images
    │   │
    │   ├── Program.cs                 # Application entry point
    │   ├── appsettings.json           # Configuration (template)
    │   ├── appsettings.Development.json # Development settings
    │   └── TafsilkPlatform.Web.csproj # Project file
    │
    ├── TafsilkPlatform.Models/        # Domain models & ViewModels
    │   ├── Models/                    # Entity models
    │   │   ├── User.cs                # User entity
    │   │   ├── Role.cs                # Role entity
    │   │   ├── CustomerProfile.cs     # Customer profile
    │   │   ├── TailorProfile.cs       # Tailor profile
    │   │   ├── Order.cs               # Order entity
    │   │   ├── OrderItem.cs           # Order line items
    │   │   ├── OrderImages.cs         # Order reference images
    │   │   ├── Product.cs             # Product entity
    │   │   ├── ShoppingCart.cs        # Shopping cart
    │   │   ├── CartItem.cs            # Cart items
    │   │   ├── Payment.cs             # Payment records
    │   │   ├── TailorService.cs       # Tailor services offered
    │   │   ├── PortfolioImage.cs      # Portfolio images
    │   │   └── Address.cs             # User addresses
    │   │
    │   └── ViewModels/                # Data transfer objects
    │       ├── Account/               # Authentication ViewModels
    │       ├── Orders/                # Order ViewModels
    │       ├── Store/                 # Store ViewModels
    │       ├── Dashboards/            # Dashboard ViewModels
    │       ├── Profiles/              # Profile ViewModels
    │       └── Payment/               # Payment ViewModels
    │
    ├── TafsilkPlatform.DataAccess/    # Data access layer
    │   ├── Data/                      # Database context & seeding
    │   │   ├── ApplicationDbContext.cs # Main DbContext
    │   │   ├── DbInitializer.cs       # Database initialization
    │   │   └── Seed/                  # Seed data
    │   │       ├── AdminSeeder.cs
    │   │       ├── UserSeeder.cs
    │   │       ├── TailorSeeder.cs
    │   │       ├── ProductSeeder.cs
    │   │       └── PortfolioSeeder.cs
    │   │
    │   ├── Repository/                # Repository pattern
    │   │   ├── IRepository.cs         # Generic repository interface
    │   │   ├── EfRepository.cs        # Generic repository implementation
    │   │   ├── IUnitOfWork.cs         # Unit of work interface
    │   │   ├── UnitOfWork.cs          # Unit of work implementation
    │   │   ├── OrderRepository.cs
    │   │   ├── ProductRepository.cs
    │   │   ├── TailorRepository.cs
    │   │   ├── CustomerRepository.cs
    │   │   ├── PaymentRepository.cs
    │   │   └── ShoppingCartRepository.cs
    │   │
    │   ├── Migrations/                # EF Core migrations
    │   │   └── [Timestamp]_*.cs       # Migration files
    │   │
    │   └── DesignTime/                # Design-time services
    │       └── DesignTimeDbContextFactory.cs
    │
    ├── TafsilkPlatform.Utility/       # Shared utilities
    │   ├── Helpers/                   # Helper classes
    │   ├── Extensions/                # Extension methods
    │   └── Constants/                 # Application constants
    │
    └── TafsilkPlatformSolution.sln    # Visual Studio solution file
```

---

## 🗄️ Database Schema

### Core Entities

#### Users & Authentication

- **`Users`**: User accounts with authentication credentials
  - `UserId` (Guid, PK)
  - `Email`, `UserName`, `PasswordHash`, `PhoneNumber`
  - `RoleId` (FK to Roles)
  - `IsEmailConfirmed`, `IsTwoFactorEnabled`

- **`Roles`**: User roles (Admin, Customer, Tailor)
  - `Id` (Guid, PK)
  - `Name`, `Description`, `Permissions` (JSON)

- **`CustomerProfiles`**: Extended customer information
  - `Id` (Guid, PK)
  - `UserId` (FK to Users)
  - `FullName`, `Gender`, `PhoneNumber`, `DateOfBirth`

- **`TailorProfiles`**: Extended tailor information
  - `Id` (Guid, PK)
  - `UserId` (FK to Users)
  - `ShopName`, `Biography`, `YearsOfExperience`
  - `City`, `District`, `Latitude`, `Longitude`
  - `ProfilePictureData` (byte[])

#### Orders & Transactions

- **`Orders`**: Custom tailoring & store orders
  - `OrderId` (Guid, PK)
  - `CustomerId` (FK), `TailorId` (FK)
  - `OrderType` (Custom/Store), `Status` (Enum)
  - `TotalPrice`, `CommissionAmount`, `CommissionRate`
  - `DueDate`, `DeliveryAddress`

- **`OrderItems`**: Individual items within orders
  - `OrderItemId` (Guid, PK)
  - `OrderId` (FK), `ProductId` (FK, nullable)
  - `Description`, `Quantity`, `UnitPrice`, `Total`
  - `SelectedSize`, `SelectedColor`

- **`OrderImages`**: Reference images for custom orders
  - `OrderImageId` (Guid, PK)
  - `OrderId` (FK)
  - `ImageData` (byte[]), `ContentType`, `ImgUrl`

#### E-Commerce

- **`Products`**: Tailoring products for sale
  - `ProductId` (Guid, PK)
  - `Name`, `Description`, `Price`, `DiscountedPrice`
  - `Category`, `SubCategory`, `Brand`, `Material`
  - `StockQuantity`, `IsAvailable`, `IsFeatured`
  - `TailorId` (FK, nullable - for tailor-created products)

- **`ShoppingCarts`**: Customer shopping carts
  - `CartId` (Guid, PK)
  - `CustomerId` (FK)
  - `CreatedAt`, `ExpiresAt`, `IsActive`

- **`CartItems`**: Items in shopping cart
  - `CartItemId` (Guid, PK)
  - `CartId` (FK), `ProductId` (FK)
  - `Quantity`, `UnitPrice`

#### Payments

- **`Payments`**: Payment records
  - `PaymentId` (Guid, PK)
  - `OrderId` (FK), `CustomerId` (FK), `TailorId` (FK)
  - `Amount`, `PaymentType` (Stripe/Cash)
  - `Status` (Pending/Completed/Failed/Refunded)
  - `TransactionId`, `Provider`, `PaidAt`

#### Tailor Features

- **`TailorServices`**: Services offered by tailors
  - `TailorServiceId` (Guid, PK)
  - `TailorId` (FK)
  - `ServiceName`, `Description`, `BasePrice`
  - `EstimatedDuration`

- **`PortfolioImages`**: Tailor portfolio showcase
  - `PortfolioImageId` (Guid, PK)
  - `TailorId` (FK)
  - `Title`, `Category`, `Description`
  - `ImageData` (byte[]), `ContentType`
  - `DisplayOrder`

#### Supporting Entities

- **`Addresses`**: User delivery addresses
  - `AddressId` (Guid, PK)
  - `UserId` (FK)
  - `Street`, `City`, `State`, `PostalCode`
  - `IsDefault`

---

## 🚦 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- ✅ **.NET 9.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- ✅ **SQL Server 2019+** or **SQL Server Express** ([Download](https://www.microsoft.com/sql-server/sql-server-downloads))
  - Alternatively, **SQLite** for development (included in project)
- ✅ **Visual Studio 2022** or **VS Code** with C# extension
- ✅ **Git** for version control
- ✅ (Optional) **Stripe CLI** for webhook testing ([Download](https://stripe.com/docs/stripe-cli))

### Installation Steps

#### 1. Clone the Repository

```bash
git clone <repository-url>
cd Tafsilk/TafsilkPlatform/TafsilkPlatform.Web
```

#### 2. Configure Database Connection

**Option A: SQL Server (Recommended for Production)**

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TafsilkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Option B: SQLite (Quick Start)**

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tafsilk-dev.db"
  }
}
```

#### 3. Apply Database Migrations

```bash
# From TafsilkPlatform.Web directory
dotnet ef database update
```

This will create the database schema and seed initial data.

#### 4. Configure External Services (Optional but Recommended)

**Google OAuth** (for social login):

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create OAuth 2.0 credentials
3. Add to `appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

**Stripe** (for payments):

1. Create account at [Stripe](https://stripe.com/)
2. Get test API keys from dashboard
3. Add to `appsettings.Development.json`:

```json
{
  "Payment": {
    "Stripe": {
      "Enabled": true,
      "PublishableKey": "pk_test_...",
      "SecretKey": "sk_test_...",
      "WebhookSecret": "whsec_...",
      "PaymentMethodConfigurationId": "pmc_..."
    }
  }
}
```

**Email Service** (SMTP or SendGrid):

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@tafsilk.com",
    "FromName": "Tafsilk Platform",
    "EnableSsl": true
  }
}
```

#### 5. Run the Application

```bash
dotnet run
```

Or use:

```bash
dotnet watch run  # For hot reload during development
```

#### 6. Access the Application

Navigate to:
- **HTTPS**: `https://localhost:7186`
- **HTTP**: `http://localhost:5186`

### Default Test Accounts

After database seeding, use these accounts:

| Role     | Email                  | Password    |
|----------|------------------------|-------------|
| Admin    | admin@tafsilk.com      | Admin@123   |
| Customer | customer@test.com      | Test@123    |
| Tailor   | tailor@test.com        | Test@123    |

---

## ⚙️ Configuration

### Configuration Files

- **`appsettings.json`**: Base configuration (template with placeholders)
- **`appsettings.Development.json`**: Development environment settings
- **`appsettings.Production.json`**: Production settings (git-ignored)

### Key Configuration Sections

#### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TafsilkDb;..."
  }
}
```

#### Application Settings

```json
{
  "Application": {
    "Name": "Tafsilk Platform",
    "Version": "1.0.0",
    "SupportEmail": "support@tafsilk.com",
    "BaseUrl": "https://localhost:7186"
  }
}
```

#### Features Toggle

```json
{
  "Features": {
    "EnableGoogleOAuth": true,
    "EnableEmailVerification": true,
    "EnableSmsNotifications": false,
    "EnableRequestLogging": true,
    "EnableResponseCaching": true
  }
}
```

#### File Upload Settings

```json
{
  "FileUpload": {
    "MaxFileSizeBytes": 10485760,        // 10 MB
    "MaxImageSizeBytes": 5242880,        // 5 MB
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "AllowedDocumentExtensions": [".pdf", ".doc", ".docx"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

#### Security Settings

```json
{
  "Security": {
    "MaxLoginAttempts": 5,
    "LockoutMinutes": 15,
    "PasswordResetTokenExpirationHours": 1,
    "EmailVerificationTokenExpirationHours": 24,
    "RequireEmailVerification": true,
    "RequireTwoFactorForAdmin": false
  }
}
```

#### Performance Settings

```json
{
  "Performance": {
    "DefaultPageSize": 20,
    "MaxPageSize": 100,
    "CacheDurationMinutes": 30,
    "EnableResponseCompression": true,
    "EnableQuerySplitting": true
  }
}
```

### User Secrets (Recommended for Development)

For sensitive data, use .NET User Secrets:

```bash
# Initialize user secrets
dotnet user-secrets init

# Set individual secrets
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_..."
```

---

## 👥 Features by Role

### Customer Features

1. **Account Management**
   - Register/Login (Email + Password or Google OAuth)
   - Profile completion with personal details
   - Address management (multiple addresses, default)
   - Password reset & email verification

2. **Tailor Discovery**
   - Browse all registered tailors
   - Filter by location (city, district)
   - Filter by services offered
   - Sort by rating, reviews, experience
   - View tailor profiles with portfolio

3. **Order Creation**
   - Select tailor and services
   - Add measurements and specifications
   - Upload reference images
   - Set delivery preferences
   - Request delivery date

4. **Store Shopping**
   - Browse product catalog
   - Filter by category, price, brand
   - Search products
   - Add to cart with size/color selection
   - Shopping cart management
   - Multi-tailor checkout (automatic tailor assignment)

5. **Payment**
   - Stripe checkout (Card, Cash App, etc.)
   - Cash on Delivery option
   - Order confirmation emails

6. **Order Tracking**
   - View order status timeline
   - Track all orders (custom + store)
   - View order details and items
   - Download invoices

7. **Dashboard**
   - Recent orders summary
   - Quick actions
   - Order statistics

### Tailor Features

1. **Profile Management**
   - Complete business profile (shop name, bio, experience)
   - Set location (city, district, coordinates)
   - Upload profile picture
   - Manage contact information

2. **Portfolio**
   - Upload work samples
   - Categorize portfolio images
   - Add titles and descriptions
   - Reorder display

3. **Service Management**
   - Define services offered (alterations, custom design, etc.)
   - Set base pricing per service
   - Specify estimated duration

4. **Product Management**
   - List products for sale
   - Set pricing, stock, categories
   - Upload product images
   - Manage inventory

5. **Order Management**
   - View incoming custom orders
   - Update order status (Pending → Processing → Completed → Delivered)
   - View order details, measurements, reference images
   - Communicate delivery updates

6. **Dashboard Analytics**
   - Revenue tracking (total, monthly)
   - Order statistics (total, pending, completed)
   - Commission calculations
   - Recent orders overview
   - Performance metrics

### Admin Features

1. **User Management**
   - View all users (Customers, Tailors, Admins)
   - Filter and search users
   - View user details and activity
   - Manage user roles
   - Lock/unlock accounts

2. **Order Management**
   - View all platform orders
   - Filter by status, type, date
   - Order details and tracking
   - Intervene in disputes

3. **Product Management**
   - View all products
   - Approve/reject product listings
   - Edit product details
   - Remove inappropriate content

4. **Dashboard**
   - Platform-wide statistics
   - Total users, orders, revenue
   - Growth metrics
   - Activity logs

5. **Commission Settings**
   - Configure platform commission rate
   - View commission breakdown

6. **System Monitoring**
   - Activity logs
   - Error tracking
   - Performance monitoring

---

## 🌐 API Endpoints

### Public Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Home page |
| GET | `/Tailors` | Browse tailors |
| GET | `/Tailors/Details/{id}` | Tailor profile |
| GET | `/Store` | Product catalog |
| GET | `/Store/Details/{id}` | Product details |
| GET | `/Account/Login` | Login page |
| POST | `/Account/Login` | Authenticate user |
| POST | `/Profiles/UpdateProfile` | Update profile |

### Tailor Endpoints (Requires Tailor Role)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Dashboards/Tailor` | Tailor dashboard |
| GET | `/Tailor/TailorManagement/Orders` | Manage orders |
| POST | `/Tailor/TailorManagement/UpdateOrderStatus` | Update order |
| GET | `/Tailor/TailorManagement/ManageProducts` | Product management |
| POST | `/Tailor/TailorManagement/AddProduct` | Create product |
| GET | `/Tailor/TailorManagement/Portfolio` | Portfolio management |
| POST | `/Tailor/TailorManagement/UploadPortfolio` | Upload portfolio image |

### Admin Endpoints (Requires Admin Role)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/AdminDashboard` | Admin dashboard |
| GET | `/AdminDashboard/Users` | User management |
| GET | `/AdminDashboard/Orders` | Order oversight |
| GET | `/AdminDashboard/ActivityLogs` | Activity logs |

### RESTful API (JWT Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | JWT login |
| POST | `/api/auth/refresh` | Refresh token |
| GET | `/api/orders` | List orders (JSON) |
| GET | `/api/orders/{id}` | Get order (JSON) |
| PUT | `/api/orders/{id}/status` | Update order status |

### Payment Webhooks

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/Payments/Webhook` | Stripe webhook handler |

---

## 💳 Payment Integration

### Stripe Configuration

The platform uses **Stripe** for secure payment processing with the following features:

- **Payment Methods**: Card, Cash App Pay, and more (via Stripe Checkout)
- **Webhook Events**: Automated payment confirmation
- **Security**: PCI-compliant payment handling

### Payment Flow

1. **Customer Checkout**:
   - Customer completes cart and initiates checkout
   - Multi-tailor orders are split automatically
   - System creates pending orders

2. **Payment Selection**:
   - **Stripe**: Redirects to Stripe Checkout
   - **Cash on Delivery**: Order marked as pending payment

3. **Stripe Payment**:
   - Customer completes payment on Stripe hosted page
   - Stripe sends webhook event to platform
   - PaymentProcessorService confirms payment
   - Order status updated to paid

4. **Commission Calculation**:
   - Platform commission deducted from tailor's revenue
   - Commission rate configurable per order
   - Tracked in `Orders.CommissionAmount` and `Payments` table

### Testing Payments

**Test Card Numbers** (Stripe Test Mode):

- Success: `4242 4242 4242 4242`
- Decline: `4000 0000 0000 0002`
- CVC: Any 3 digits
- Expiry: Any future date

### Cash on Delivery

- Available as alternative payment method
- Order created as pending payment
- Payment collected by tailor upon delivery
- Status manually updated by tailor/admin

---

## 🔐 Security

### Authentication & Authorization

- **ASP.NET Core Identity**: Secure password hashing (PBKDF2)
- **Google OAuth 2.0**: Social login integration
- **JWT Tokens**: API authentication for mobile/SPA clients
- **Role-Based Access Control**: Admin, Tailor, Customer roles
- **Email Verification**: Prevent fake accounts
- **Password Requirements**: Minimum length, complexity rules

### Security Features

- ✅ **Anti-Forgery Tokens**: CSRF protection on all forms
- ✅ **HTTPS Enforcement**: SSL/TLS for all connections
- ✅ **Input Validation**: FluentValidation + Data Annotations
- ✅ **SQL Injection Prevention**: Parameterized queries (EF Core)
- ✅ **XSS Protection**: Razor encoding, Content Security Policy
- ✅ **Rate Limiting**: Login attempt limits, account lockout
- ✅ **Secure Headers**: X-Frame-Options, X-Content-Type-Options
- ✅ **Dependency Scanning**: Regular NuGet package updates

### Data Protection

- **Sensitive Data**: Stored in User Secrets / Environment Variables
- **File Upload Validation**: Extension and size checks
- **Binary Storage**: Profile pictures and images stored as byte arrays
- **Logging**: PII excluded from logs (Serilog configuration)

### Best Practices

1. **Never commit secrets** to version control
2. Use **Environment Variables** or **Azure Key Vault** in production
3. Rotate **API keys** regularly
4. Enable **Two-Factor Authentication** for admin accounts
5. Monitor **activity logs** for suspicious behavior

---

## 🚀 Deployment

### Production Checklist

- [ ] Update `appsettings.Production.json` with production connection strings
- [ ] Store secrets in **Azure Key Vault**, **AWS Secrets Manager**, or **Environment Variables**
- [ ] Enable **HTTPS** with valid SSL certificate
- [ ] Configure **production logging** (Application Insights, Serilog to cloud)
- [ ] Set up **database backups** (automated daily backups)
- [ ] Configure **CDN** for static files (optional: Azure CDN, Cloudflare)
- [ ] Enable **health checks** endpoint
- [ ] Set up **monitoring and alerting** (Application Insights, Datadog)
- [ ] Configure **auto-scaling** (if using cloud hosting)
- [ ] Test **Stripe webhooks** in production environment

### Build for Production

```bash
# Publish self-contained application
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Or framework-dependent (requires .NET runtime on server)
dotnet publish -c Release -o ./publish
```

### Database Migration in Production

```bash
# Apply migrations on production server
dotnet ef database update --project TafsilkPlatform.Web --configuration Release
```

### Hosting Options

- **IIS** (Windows Server)
- **Azure App Service**
- **AWS Elastic Beanstalk**
- **Docker** (containerized deployment)
- **Linux with Nginx/Apache** (reverse proxy)

### Environment Variables (Production)

Set these on your hosting platform:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<production-db-connection>
Authentication__Google__ClientId=<google-client-id>
Authentication__Google__ClientSecret=<google-client-secret>
Payment__Stripe__SecretKey=<stripe-secret-key>
Email__SmtpPassword=<smtp-password>
```

---

## 💻 Development Guide

### Coding Standards

- **C# Conventions**: Follow Microsoft C# coding guidelines
- **Naming**: PascalCase for classes/methods, camelCase for variables
- **Comments**: XML documentation for public APIs
- **Async/Await**: Use async methods for I/O operations
- **SOLID Principles**: Write maintainable, testable code

### Project Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run --project TafsilkPlatform.Web

# Run with hot reload
dotnet watch run --project TafsilkPlatform.Web

# Create new migration
dotnet ef migrations add MigrationName --project TafsilkPlatform.Web

# Update database
dotnet ef database update --project TafsilkPlatform.Web

# Rollback migration
dotnet ef database update PreviousMigrationName --project TafsilkPlatform.Web

# Drop database (WARNING: Deletes all data)
dotnet ef database drop --project TafsilkPlatform.Web
```

### Adding a New Feature

1. **Create Model** (TafsilkPlatform.Models)
2. **Update DbContext** (ApplicationDbContext.cs)
3. **Create Migration** (`dotnet ef migrations add...`)
4. **Create Repository** (if needed)
5. **Create Service** (business logic)
6. **Create Controller** (MVC or API)
7. **Create Views** (Razor pages)
8. **Test Feature**

### Debugging

- Use **Visual Studio Debugger** or **VS Code Debugger**
- Enable **Developer Exception Page** in development
- Check **Logs** in `Logs/` directory (Serilog)
- Use **Browser Developer Tools** for frontend issues

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **Unit Tests**: Service layer, business logic
- **Integration Tests**: Database operations, API endpoints
- **Browser Tests**: Use browser_subagent tool for UI testing

*(Note: Test projects not yet implemented. Placeholder for future development.)*

---

## 🤝 Contributing

### How to Contribute

1. **Fork** the repository
2. Create a **feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. Open a **Pull Request**

### Pull Request Guidelines

- Write clear, concise commit messages
- Include screenshots for UI changes
- Update documentation if needed
- Ensure all tests pass
- Follow existing code style

---

## 📄 License

This project is **proprietary software**. All rights reserved.

**Copyright © 2025 Tafsilk Platform**

Unauthorized copying, modification, distribution, or use of this software is strictly prohibited.

---

## 👥 Team

- **Lead Developer**: Ahmed Tyson
- **Project Type**: Tailoring Services Marketplace Platform
- **Purpose**: Academic/Commercial Project

---

## 📞 Support

For support, questions, or feature requests:

- **Email**: support@tafsilk.com
- **Issues**: Create an issue in the repository
- **Documentation**: Refer to inline code documentation

---

## 🔄 Version History

### Version 1.0.0 (December 2025)

**Initial Release**

- ✅ User authentication (Email + Google OAuth)
- ✅ Customer & Tailor profile management
- ✅ Custom order creation and tracking
- ✅ E-commerce store with shopping cart
- ✅ Multi-tailor checkout with automatic assignment
- ✅ Stripe payment integration
- ✅ Tailor portfolio management
- ✅ Product management for tailors
- ✅ Order status workflow (Pending → Processing → Completed → Delivered)
- ✅ Admin dashboard with analytics
- ✅ Commission tracking and calculation
- ✅ Responsive UI with premium design
- ✅ Email notifications (SendGrid/SMTP)
- ✅ Comprehensive logging (Serilog)
- ✅ RESTful API with JWT authentication

---

## 🙏 Acknowledgments

- **ASP.NET Core Team** for the excellent framework
- **Stripe** for payment infrastructure
- **Google** for OAuth services
- **Bootstrap** for UI components
- **Font Awesome** for icons

---

**Built with ❤️ for the tailoring community**

*Empowering tailors, delighting customers, one stitch at a time.*
