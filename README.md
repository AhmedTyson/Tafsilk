# Tafsilk Platform

A comprehensive tailoring services platform built with ASP.NET Core MVC, connecting customers with professional tailors and providing an integrated e-commerce store for tailoring products.

## 🌟 Features

### For Customers
- **Browse Tailors**: Search and filter professional tailors by location, services, and ratings
- **Custom Orders**: Create custom tailoring orders with specific measurements and requirements
- **Product Store**: Purchase tailoring supplies, fabrics, and accessories
- **Order Tracking**: Real-time order status updates and delivery tracking
- **Profile Management**: Manage personal information, saved addresses, and preferences

### For Tailors
- **Business Profile**: Showcase services, portfolio, and expertise
- **Order Management**: Receive and manage customer orders efficiently
- **Product Listing**: Sell tailoring products through the integrated store
- **Dashboard Analytics**: Track revenue, orders, and business performance

### For Administrators
- **User Management**: Manage customers, tailors, and their profiles
- **Order Oversight**: Monitor all platform orders and transactions
- **Dashboard**: Comprehensive platform statistics and analytics

## 🚀 Technology Stack

- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: SQL Server (with SQLite for development)
- **Authentication**: ASP.NET Core Identity with Google OAuth
- **Frontend**: Bootstrap 5, Font Awesome, Custom CSS
- **Image Storage**: Local file system with URL-based serving
- **Payment**: Stripe integration (configured but simplified)

## 📁 Project Structure

```
TafsilkPlatform/
├── TafsilkPlatform.Web/           # Main web application
│   ├── Areas/                     # Feature areas (Customer, Tailor)
│   ├── Controllers/               # MVC controllers
│   ├── Views/                     # Razor views
│   ├── wwwroot/                   # Static files (CSS, JS, images)
│   ├── Services/                  # Application services
│   ├── BLL/                       # Business logic layer
│   ├── Middleware/                # Custom middleware
│   └── Program.cs                 # Application entry point
├── TafsilkPlatform.Models/        # Data models and ViewModels
├── TafsilkPlatform.DataAccess/    # Database context and migrations
└── TafsilkPlatform.Utility/       # Utility classes and helpers
```

## 🛠️ Development Setup

### Prerequisites
- .NET 9.0 SDK or later
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or VS Code
- Git

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Tafsilk/TafsilkPlatform
   ```

2. **Configure the database**
   
   Update `appsettings.Development.json` with your connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=TafsilkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project TafsilkPlatform.Web
   ```

4. **Configure Google OAuth (Optional)**
   
   In `appsettings.Development.json`, add your Google OAuth credentials:
   ```json
   {
     "Authentication": {
       "Google": {
         "ClientId": "your-client-id",
         "ClientSecret": "your-client-secret"
       }
     }
   }
   ```

5. **Run the application**
   ```bash
   cd TafsilkPlatform.Web
   dotnet run
   ```

6. **Access the application**
   
   Navigate to `https://localhost:5001` in your browser

### Default Test Accounts

After applying migrations, seed data creates default accounts:
- **Admin**: admin@tafsilk.com / Admin@123
- **Customer**: customer@test.com / Test@123
- **Tailor**: tailor@test.com / Test@123

## 🔧 Configuration

### Environment Variables

Create environment-specific configuration files:
- `appsettings.Development.json` - Development settings
- `appsettings.Production.json` - Production settings (git-ignored)

### Key Configuration Sections

- **ConnectionStrings**: Database connection configuration
- **Authentication**: OAuth providers (Google)
- **FileUpload**: Image storage settings
- **Logging**: Application logging configuration
- **AllowedHosts**: Allowed host headers

## 📦 Deployment

### Production Checklist

- [ ] Update `appsettings.Production.json` with production connection strings
- [ ] Configure secure secret storage (Azure Key Vault, AWS Secrets Manager)
- [ ] Enable HTTPS with valid SSL certificates
- [ ] Configure production logging (Application Insights, Serilog)
- [ ] Set up database backups
- [ ] Configure CDN for static files (optional)
- [ ] Enable health checks endpoint
- [ ] Set up monitoring and alerting

### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

### Database Migration in Production

```bash
dotnet ef database update --project TafsilkPlatform.Web --configuration Release
```

## 🏗️ Architecture

### Layered Architecture

1. **Presentation Layer** (`TafsilkPlatform.Web`)
   - Controllers, Views, Areas
   - ViewModels for data transfer
   - Client-side assets

2. **Business Logic Layer** (`BLL` & `Services`)
   - Service interfaces and implementations
   - Business rules and validation
   - Domain logic

3. **Data Access Layer** (`TafsilkPlatform.DataAccess`)
   - Entity Framework Core DbContext
   - Database migrations
   - Repository pattern (if implemented)

4. **Domain Layer** (`TafsilkPlatform.Models`)
   - Entity models
   - DTOs and ViewModels
   - Enumerations

### Design Patterns Used

- **Dependency Injection**: Service registration and resolution
- **Repository Pattern**: Data access abstraction (partial)
- **MVC Pattern**: Model-View-Controller architecture
- **Service Layer**: Business logic encapsulation

## 🔐 Security Features

- ASP.NET Core Identity for authentication
- Role-based authorization (Admin, Customer, Tailor)
- HTTPS enforcement
- Anti-forgery tokens for forms
- Input validation and sanitization
- Secure password hashing
- CORS policy configuration

## 📝 API Endpoints

### Public Routes
- `GET /` - Home page
- `GET /Tailors` - Browse tailors
- `GET /Account/Login` - Login page
- `GET /Account/Register` - Registration page

### Customer Routes (Requires Authentication)
- `GET /Dashboards/Customer` - Customer dashboard
- `GET /Orders/MyOrders` - View orders
- `GET /Orders/CreateOrder/{tailorId}` - Create new order
- `GET /Store` - Product store
- `GET /Store/Cart` - Shopping cart

### Tailor Routes (Requires Tailor Role)
- `GET /Dashboards/Tailor` - Tailor dashboard
- `GET /Tailor/TailorManagement/ManageProducts` - Manage products
- `GET /Tailor/TailorManagement/Portfolio` - Portfolio management

### Admin Routes (Requires Admin Role)
- `GET /AdminDashboard` - Admin dashboard
- `GET /AdminDashboard/Users` - User management
- `GET /AdminDashboard/Orders` - Order management

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 Additional Documentation

- [Deployment Guide](./docs/DEPLOYMENT.md) - Detailed deployment instructions
- [Development Guide](./docs/DEVELOPMENT.md) - Development best practices
- [API Documentation](./docs/API.md) - API endpoint reference

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is proprietary software. All rights reserved.

## 👥 Team

- **Development Team**: Ahmed Tyson
- **Project Type**: Tailoring Services Platform

## 📞 Support

For support and questions:
- **Email**: support@tafsilk.com
- **Issues**: Create an issue in the repository

## 🔄 Version History

### Version 1.0.0 (Current)
- Initial release
- Core features: User management, Order system, Product store
- Authentication with Google OAuth
- Responsive UI with premium design
- Admin dashboard and analytics

---

**Built with ❤️ for the tailoring community**
