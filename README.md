# Tafsilk Platform - منصة تفصيلك

[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)

**Tafsilk** is a comprehensive tailoring marketplace platform that connects skilled tailors with customers and corporate clients across Egypt.

## 🎯 Features

### For Customers
- 🔍 **Search & Discover** - Find verified tailors by location, specialty, and ratings
- 📋 **Order Management** - Create and track custom tailoring orders
- 💬 **RFQ System** - Request quotes from multiple tailors
- ⭐ **Reviews & Ratings** - Rate and review tailor services
- 📱 **Profile Management** - Manage personal information and addresses
- 💰 **Wallet System** - Secure payment and transaction management

### For Tailors
- 👔 **Portfolio Showcase** - Display your work and attract customers
- 📊 **Dashboard** - Track orders, earnings, and performance metrics
- 💼 **Service Management** - Define services and pricing
- 📈 **Analytics** - View business insights and customer feedback
- ✅ **Verification System** - Get verified badge for credibility
- 📸 **Image Gallery** - Showcase your best work

### For Corporate Clients
- 🏢 **Bulk Orders** - Place large-scale uniform orders
- 📑 **Contract Management** - Manage long-term agreements
- 👥 **Team Management** - Multiple user accounts per organization
- 📊 **Reporting** - Detailed analytics and order tracking

### For Administrators
- 🛡️ **User Management** - Manage all user accounts and roles
- ✔️ **Tailor Verification** - Review and approve tailor registrations
- 🔍 **Dispute Resolution** - Handle customer-tailor disputes
- 📊 **Platform Analytics** - Monitor platform health and usage
- ⚙️ **System Configuration** - Manage platform settings

## 🏗️ Architecture

### Technology Stack
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: Cookie-based + JWT Bearer tokens
- **OAuth Providers**: Google, Facebook
- **Validation**: FluentValidation
- **Patterns**: Repository Pattern, Unit of Work

### Project Structure
```
TafsilkPlatform.Web/
├── Controllers/        # MVC Controllers
├── Data/     # DbContext, Migrations, Seeders
├── Extensions/           # Extension methods
├── Interfaces/           # Service/Repository interfaces
├── Middleware/           # Custom middleware
├── Models/     # Domain models
├── Repositories/         # Data access layer
├── Security/ # Authentication, Authorization
├── Services/  # Business logic
├── ViewModels/         # View models
├── Views/             # Razor views
└── wwwroot/     # Static files
```

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or Visual Studio Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/AhmedTyson/Tafsilk.git
   cd Tafsilk
   ```

2. **Configure the database**
   
   Update the connection string in `appsettings.json` if needed:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb;Trusted_Connection=True;"
   }
 ```

3. **Set up user secrets** (for OAuth and sensitive data)
   ```bash
   cd TafsilkPlatform.Web
   
   # JWT Secret
   dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_KEY_HERE"
   
   # Google OAuth
   dotnet user-secrets set "Google:client_id" "YOUR_GOOGLE_CLIENT_ID"
   dotnet user-secrets set "Google:client_secret" "YOUR_GOOGLE_CLIENT_SECRET"
   
   # Facebook OAuth
   dotnet user-secrets set "Facebook:app_id" "YOUR_FACEBOOK_APP_ID"
   dotnet user-secrets set "Facebook:app_secret" "YOUR_FACEBOOK_APP_SECRET"
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project TafsilkPlatform.Web
   ```

5. **Run the application**
   ```bash
   dotnet run --project TafsilkPlatform.Web
   ```

   The application will be available at:
   - HTTPS: `https://localhost:7001`
   - HTTP: `http://localhost:5000`

### Default Admin Account
On first run, a default admin account is created:
- **Email**: `admin@tafsilk.com`
- **Password**: `Admin@123`

⚠️ **Change this password immediately in production!**

## 🔧 Configuration

### OAuth Setup

#### Google OAuth
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URI: `https://yourdomain.com/signin-google`
6. Copy Client ID and Client Secret to user secrets

#### Facebook OAuth
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app
3. Add Facebook Login product
4. Configure OAuth redirect URIs
5. Copy App ID and App Secret to user secrets

### Email Configuration
Configure SMTP settings for email notifications:
```bash
dotnet user-secrets set "Email:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
```

## 📦 Deployment

### Publish the Application
```bash
dotnet publish -c Release -o ./publish
```

### Database Migration in Production
```bash
dotnet ef database update --project TafsilkPlatform.Web --configuration Release
```

### Environment Variables (Production)
Set these environment variables on your hosting platform:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Google__client_id`
- `Google__client_secret`
- `Facebook__app_id`
- `Facebook__app_secret`

## 🧪 Testing

Run tests (once test project is added):
```bash
dotnet test
```

## 📝 API Documentation

API endpoints are documented and accessible at:
- Development: `https://localhost:7001/swagger`
- Production: Available when Swagger is enabled

### Key API Endpoints

#### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout

#### Orders
- `GET /api/orders` - List orders
- `POST /api/orders` - Create order
- `GET /api/orders/{id}` - Get order details
- `PUT /api/orders/{id}` - Update order

## 🛠️ Development

### Code Style
- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable names
- Add XML documentation for public APIs
- Keep methods small and focused

### Database Migrations
Create a new migration:
```bash
dotnet ef migrations add MigrationName --project TafsilkPlatform.Web
```

Remove last migration:
```bash
dotnet ef migrations remove --project TafsilkPlatform.Web
```

### Debug Logging
Enable debug logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
    "Microsoft.AspNetCore": "Information"
    }
  }
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

Copyright © 2025 Tafsilk Platform. All rights reserved.

This is proprietary software. Unauthorized copying, distribution, or modification is strictly prohibited.

## 👥 Team

- **Ahmed Tyson** - Lead Developer

## 📞 Support

For support and inquiries:
- Email: support@tafsilk.com
- GitHub Issues: [Report an issue](https://github.com/AhmedTyson/Tafsilk/issues)

## 🗺️ Roadmap

- [ ] Mobile applications (iOS & Android)
- [ ] Real-time chat between customers and tailors
- [ ] AI-powered tailor recommendations
- [ ] Video consultation feature
- [ ] Multi-language support (Arabic, English)
- [ ] Integration with shipping providers
- [ ] Advanced analytics dashboard
- [ ] Loyalty program

## 📊 Status

🚧 **Active Development** - This project is under active development.

---

Made with ❤️ in Egypt 🇪🇬
