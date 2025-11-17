# Tafsilk Platform - ASP.NET MVC Demo Project

## Overview
This is a demonstration ASP.NET Core MVC project that showcases a tailor platform connecting customers with tailors. The project implements **real authentication** while using **mock/fake data** for all other features.

## Features

### ✅ Real Authentication (Fully Implemented)
- **Login/Logout** with real password validation
- **Registration** with validation
- **Cookie-based authentication**
- **Role-based authorization** (Customer, Tailor, Admin)
- **Session management**
- Password hashing using SHA256

### 📊 Mock Data Features
All non-authentication features use static/fake data:
- Tailor profiles listing and details
- Customer profiles
- Services offered by tailors
- Orders and order tracking
- Dashboard statistics (Admin only)

## Demo Accounts

### Customer Account
- **Email:** customer@test.com
- **Password:** 123456

### Tailor Account
- **Email:** tailor@test.com
- **Password:** 123456

### Admin Account
- **Email:** admin@test.com
- **Password:** admin123

## Project Structure

```
TafsilkPlatform.MVC/
├── Controllers/
│   ├── AccountController.cs       # Authentication (REAL)
│   ├── HomeController.cs   # Home page with featured tailors
│   ├── TailorsController.cs       # Tailor listings and details
│   ├── OrdersController.cs   # Order management
│   └── DashboardController.cs     # Admin dashboard
│
├── Models/
│   ├── User.cs # User model for authentication
│   ├── LoginViewModel.cs          # Login form model
│   ├── RegisterViewModel.cs    # Registration form model
│   └── MockDataModels.cs          # Mock data models
│
├── Services/
│   ├── AuthService.cs             # REAL authentication service
│   └── MockDataService.cs         # Fake data provider
│
└── Views/
    ├── Account/         # Login, Register, Access Denied
    ├── Home/         # Home page
    ├── Tailors/    # Tailor listings, details, services
    ├── Orders/       # Order management
    ├── Dashboard/            # Admin dashboard
    └── Shared/        # Layout and shared views
```

## Technologies Used

- **Framework:** ASP.NET Core 9.0 MVC
- **Authentication:** Cookie Authentication
- **UI:** Bootstrap 5
- **Icons:** Bootstrap Icons
- **Language:** C# 13.0
- **Direction:** RTL (Arabic)

## How to Run

1. **Navigate to the project directory:**
   ```bash
   cd TafsilkPlatform.MVC
   ```

2. **Restore dependencies:**
   ```bash
 dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Open browser:**
   Navigate to `https://localhost:5001` or `http://localhost:5000`

## Key Endpoints

| Route | Description | Authorization |
|-------|-------------|---------------|
| `/` | Home page with featured tailors | Public |
| `/Account/Login` | Login page | Public |
| `/Account/Register` | Registration page | Public |
| `/Tailors` | List all tailors | Public |
| `/Tailors/Details/{id}` | Tailor profile details | Public |
| `/Orders` | All orders | Authenticated |
| `/Dashboard` | Admin dashboard | Admin only |

## Authentication Flow

1. **Login:** User enters email and password
2. **Validation:** Real password hashing and comparison
3. **Cookie Creation:** Authentication cookie with claims
4. **Authorization:** Role-based access control
5. **Logout:** Cookie removal and session cleanup

## Mock Data

The `MockDataService` provides:
- **3 Tailors** with different specialties
- **3 Customers** with order history
- **4 Services** offered by tailors
- **3 Sample Orders** with different statuses
- **Dashboard Statistics** for admin

## Security Features

- ✅ Password hashing (SHA256)
- ✅ Anti-forgery tokens
- ✅ Secure cookie authentication
- ✅ Role-based authorization
- ✅ Input validation
- ✅ SQL injection prevention (no database)

## Important Notes

⚠️ **This is a DEMO project** - Not production-ready
- Passwords are hashed but stored in memory
- No database connection
- Mock data is hardcoded
- No file uploads or image storage
- No payment processing

## Future Enhancements (If using real database)

- Connect to SQL Server/PostgreSQL
- Implement Entity Framework Core
- Add file upload for tailor portfolios
- Integrate payment gateway
- Add real-time notifications
- Implement reviews and ratings
- Add advanced search and filters

## License

This is a demonstration project created for educational purposes.

## Contact

For questions or support, contact the development team.
