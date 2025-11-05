# ✅ SWAGGER/OPENAPI ENABLED - COMPLETE GUIDE

## **🎊 SWAGGER SUCCESSFULLY ENABLED!**

```
✅ Swagger/OpenAPI Configured
✅ JWT Authentication Integrated
✅ Cookie Authentication Integrated
✅ Build Successful
✅ Ready to Use
```

---

## **📊 CONFIGURATION SUMMARY**

**Date:** 2025-01-20  
**Framework:** .NET 9.0  
**Swagger Package:** Swashbuckle.AspNetCore 7.2.0  
**Status:** ✅ **ENABLED IN DEVELOPMENT**

---

## **🚀 ACCESSING SWAGGER UI**

### **Development Environment:**

**Swagger UI URL:**
```
https://localhost:5001/swagger
or
http://localhost:5000/swagger
```

**Swagger JSON:**
```
https://localhost:5001/swagger/v1/swagger.json
```

### **Quick Start:**
1. Run your application: `dotnet run` or press F5
2. Open browser and navigate to: `https://localhost:5001/swagger`
3. You'll see the Swagger UI with all your API endpoints!

---

## **🔐 AUTHENTICATION IN SWAGGER**

### **Method 1: JWT Bearer Token**

1. **Get a JWT Token:**
   - Call the `/api/auth/login` endpoint (if you have one)
   - Or generate a token from your application

2. **Authorize in Swagger:**
   - Click the **"Authorize"** button (🔒 icon) at the top right
   - Enter: `Bearer YOUR_JWT_TOKEN_HERE`
   - Click **"Authorize"**
   - Now all API calls will include the JWT token

**Example:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### **Method 2: Cookie Authentication**

Cookie authentication is automatically handled for:
- Browser requests
- API calls from the same domain

**How it works:**
1. Login through your web interface (`/Account/Login`)
2. Cookie is set automatically
3. Swagger uses the same cookie for API calls

---

## **📋 SWAGGER CONFIGURATION DETAILS**

### **API Information:**
```csharp
Version: v1
Title: Tafsilk Platform API
Description: Tafsilk - منصة الخياطين والتفصيل - API Documentation
Contact: support@tafsilk.com
License: Use under Tafsilk License
```

### **Security Schemes:**

#### **1. Bearer (JWT)**
```csharp
Type: HTTP
Scheme: Bearer
Bearer Format: JWT
Location: Header (Authorization)
```

#### **2. Cookie**
```csharp
Type: API Key
Cookie Name: .Tafsilk.Auth
Location: Cookie
```

---

## **🎨 SWAGGER UI FEATURES ENABLED**

✅ **Request Duration Display** - See how long each API call takes  
✅ **Deep Linking** - Share specific endpoint URLs  
✅ **Filter** - Search for specific endpoints  
✅ **Show Extensions** - Display OpenAPI extensions  
✅ **Try It Out** - Test API endpoints directly from browser  

---

## **📝 ENABLING XML DOCUMENTATION (OPTIONAL)**

To show XML comments in Swagger UI:

### **Step 1: Enable XML Documentation in .csproj**
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn> <!-- Suppress missing XML comment warnings -->
</PropertyGroup>
```

### **Step 2: Add XML Comments to Controllers**
```csharp
/// <summary>
/// Registers a new user account
/// </summary>
/// <param name="request">User registration details</param>
/// <returns>Returns created user information</returns>
/// <response code="200">User successfully registered</response>
/// <response code="400">Invalid input data</response>
[HttpPost("register")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // Implementation
}
```

---

## **🔧 PRODUCTION CONFIGURATION**

### **Option 1: Disable in Production (Recommended)**
```csharp
// Already configured - Swagger only enabled in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { /* config */ });
}
```

### **Option 2: Enable in Production with Authentication**
```csharp
// Add authentication to Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tafsilk Platform API v1");
    
    // Require authentication
    options.OnConfigureStarted(config =>
    {
        config.ConfigObject.AdditionalItems["onComplete"] = 
            "function() { window.ui.preauthorizeApiKey('Bearer', 'YOUR_PRODUCTION_TOKEN'); }";
    });
});
```

---

## **🎯 TESTING YOUR API WITH SWAGGER**

### **Example: Test User Registration**

1. **Open Swagger UI:** `https://localhost:5001/swagger`

2. **Find Registration Endpoint:**
   - Look for `POST /api/account/register` or similar

3. **Click "Try it out"**

4. **Fill in the Request Body:**
```json
{
  "email": "test@example.com",
  "password": "Password123!",
  "fullName": "Test User",
  "phoneNumber": "0501234567",
  "role": "Customer"
}
```

5. **Click "Execute"**

6. **View Response:**
   - Status code (200, 400, etc.)
   - Response body
   - Response headers
   - Request duration

---

### **Example: Test Protected Endpoint**

1. **Get JWT Token:**
   - Login first: `POST /api/account/login`
   - Copy the token from response

2. **Authorize:**
   - Click 🔒 **"Authorize"** button
   - Enter: `Bearer YOUR_TOKEN_HERE`
   - Click **"Authorize"**

3. **Test Protected Endpoint:**
   - Find endpoint with 🔒 icon
   - Click "Try it out"
   - Execute

---

## **📊 SWAGGER ENDPOINTS OVERVIEW**

Your Swagger UI will display:

### **Account/Authentication:**
```
POST   /api/account/register
POST /api/account/login
POST   /api/account/logout
GET    /api/account/profile
PUT    /api/account/profile
POST   /api/account/change-password
```

### **Dashboards:**
```
GET    /api/dashboards/customer
GET /api/dashboards/tailor
GET    /api/dashboards/admin
```

### **Profiles:**
```
GET    /api/profiles/customer/{id}
GET    /api/profiles/tailor/{id}
PUT    /api/profiles/tailor
GET    /api/profiles/search-tailors
```

### **Orders:**
```
GET    /api/orders
POST   /api/orders
GET /api/orders/{id}
PUT    /api/orders/{id}
DELETE /api/orders/{id}
```

*(Endpoints will vary based on your actual controllers)*

---

## **🎨 CUSTOMIZING SWAGGER UI**

### **Change Theme:**
```csharp
app.UseSwaggerUI(options =>
{
    // Dark theme
options.InjectStylesheet("/swagger-ui/custom.css");
});
```

### **Custom CSS (`wwwroot/swagger-ui/custom.css`):**
```css
/* Dark theme example */
.swagger-ui {
    background-color: #1e1e1e;
    color: #ffffff;
}

.swagger-ui .topbar {
    background-color: #252526;
}
```

---

### **Add Custom Logo:**
```csharp
app.UseSwaggerUI(options =>
{
    options.HeadContent = @"
      <style>
            .topbar-wrapper img {
             content: url('/images/tafsilk-logo.png');
       height: 40px;
   }
        </style>
 ";
});
```

---

## **🔍 TROUBLESHOOTING**

### **Problem 1: Swagger Not Loading**

**Solution:**
```bash
# Check if Swagger package is installed
dotnet list package | grep Swashbuckle

# If not installed, add it:
dotnet add package Swashbuckle.AspNetCore
```

---

### **Problem 2: 404 Not Found**

**Check:**
- ✅ URL is correct: `https://localhost:5001/swagger`
- ✅ Application is running in Development mode
- ✅ `UseSwagger()` is called in Program.cs

---

### **Problem 3: No Endpoints Showing**

**Solution:**
```csharp
// Make sure you have API controllers
// Add [ApiController] attribute:
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    // Your API methods
}
```

---

### **Problem 4: JWT Not Working**

**Check:**
1. ✅ Bearer token format: `Bearer YOUR_TOKEN`
2. ✅ Token is valid and not expired
3. ✅ JWT Key matches in appsettings.json
4. ✅ Token includes required claims

**Test Token:**
```bash
# Decode JWT at https://jwt.io to verify claims
```

---

## **📱 TESTING FROM EXTERNAL TOOLS**

### **Postman:**
1. Import Swagger JSON: `https://localhost:5001/swagger/v1/swagger.json`
2. Postman will automatically create all requests

### **curl:**
```bash
# Example: Login
curl -X POST https://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}'

# Example: Protected endpoint with JWT
curl -X GET https://localhost:5001/api/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### **PowerShell:**
```powershell
# Example: Login
$body = @{
    email = "test@example.com"
    password = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/account/login" `
    -Method POST `
    -Body $body `
  -ContentType "application/json"

# Example: Protected endpoint
Invoke-RestMethod -Uri "https://localhost:5001/api/profile" `
    -Method GET `
    -Headers @{Authorization = "Bearer YOUR_TOKEN"}
```

---

## **🎯 BEST PRACTICES**

### **1. API Versioning**
```csharp
// Add versioning support
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Update Swagger to support versions
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { 
 Version = "v1", 
  Title = "Tafsilk API v1" 
    });
    
    options.SwaggerDoc("v2", new OpenApiInfo { 
        Version = "v2", 
 Title = "Tafsilk API v2" 
    });
});
```

---

### **2. Response Types**
```csharp
[HttpPost("register")]
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // Implementation
}
```

---

### **3. Add Examples**
```csharp
/// <example>
/// {
///   "email": "user@example.com",
///   "password": "Password123!",
///   "fullName": "John Doe"
/// }
/// </example>
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}
```

---

## **📋 QUICK REFERENCE**

### **URLs:**
```
Development Swagger UI:  https://localhost:5001/swagger
Swagger JSON:           https://localhost:5001/swagger/v1/swagger.json
API Base URL:           https://localhost:5001/api
```

### **Authentication:**
```
JWT Header:    Authorization: Bearer {token}
Cookie Name:   .Tafsilk.Auth
```

### **Common Status Codes:**
```
200 OK   - Success
201 Created         - Resource created
400 Bad Request     - Invalid input
401 Unauthorized    - Not authenticated
403 Forbidden       - Not authorized
404 Not Found       - Resource not found
500 Server Error    - Internal error
```

---

## **✅ VERIFICATION CHECKLIST**

- [x] ✅ Swagger package installed (Swashbuckle.AspNetCore)
- [x] ✅ AddEndpointsApiExplorer() called
- [x] ✅ AddSwaggerGen() configured
- [x] ✅ UseSwagger() added to pipeline
- [x] ✅ UseSwaggerUI() configured
- [x] ✅ JWT authentication integrated
- [x] ✅ Cookie authentication integrated
- [x] ✅ Build successful
- [x] ✅ Only enabled in Development

---

## **🎊 FINAL STATUS**

```
┌─────────────────────────────────────────────┐
│   SWAGGER/OPENAPI ENABLED  │
└─────────────────────────────────────────────┘

Configuration:         ✅ Complete
Authentication:   ✅ JWT + Cookie
Build Status:     ✅ Successful
UI Features:  ✅ All Enabled
Documentation:        ✅ Ready

Access URL:
https://localhost:5001/swagger

STATUS: ✅ READY TO USE
```

---

## **🚀 NEXT STEPS**

1. **Run your application:**
   ```bash
   dotnet run
   ```

2. **Open Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```

3. **Explore your API:**
   - View all endpoints
   - Test endpoints with "Try it out"
   - Authenticate with JWT or Cookie
   - See request/response examples

4. **Optional Enhancements:**
   - Enable XML comments
   - Add API versioning
   - Customize UI theme
   - Add examples to models

---

**Date:** 2025-01-20  
**Status:** ✅ **COMPLETE**  
**Next:** Start testing your API with Swagger UI!

---

**🎉 Swagger is now enabled and ready to use!**

**تفصيلك - نربط بينك وبين أفضل الخياطين** 🧵✂️
