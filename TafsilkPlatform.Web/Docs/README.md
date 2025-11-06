# Tafsilk Platform API Documentation
# توثيق Tafsilk Platform API

Welcome to the Tafsilk Platform API documentation! This directory contains comprehensive guides and resources for testing and integrating with the Tafsilk Platform API.

مرحباً بك في توثيق Tafsilk Platform API! يحتوي هذا الدليل على مستندات وموارد شاملة لاختبار والتكامل مع Tafsilk Platform API.

---

## 📚 Documentation Files | ملفات التوثيق

### 1. **SWAGGER_TESTING_GUIDE.md**
Complete guide for testing the API using Swagger UI.

**Contents:**
- Accessing Swagger UI
- Authentication setup (step-by-step)
- Detailed testing examples for all endpoints
- Common response codes
- Troubleshooting guide
- Best practices

**دليل كامل لاختبار API باستخدام Swagger UI**

---

### 2. **API_QUICK_REFERENCE.md**
Quick reference card for developers.

**Contents:**
- Endpoint summary table
- Sample requests
- Common errors
- Quick setup steps
- Code integration examples

**بطاقة مرجعية سريعة للمطورين**

---

### 3. **Tafsilk_API.postman_collection.json**
Ready-to-import Postman collection.

**Features:**
- Pre-configured requests for all endpoints
- Automatic token management
- Test scenarios included
- Environment variables setup
- Success/error response examples

**مجموعة Postman جاهزة للاستيراد**

---

## 🚀 Getting Started | البدء السريع

### Option 1: Use Swagger UI (Recommended for Testing)

1. **Start the application** in Development mode:
   ```bash
   dotnet run --project TafsilkPlatform.Web
   ```

2. **Open Swagger UI**:
   ```
   https://localhost:7186/swagger
   ```

3. **Follow the guide**: See `SWAGGER_TESTING_GUIDE.md` for detailed instructions

### Option 2: Use Postman

1. **Import the collection**:
   - Open Postman
   - Click **Import** → Select `Tafsilk_API.postman_collection.json`

2. **Set environment**:
   - Create new environment
   - Add variable: `baseUrl` = `https://localhost:7186`

3. **Start testing**:
   - Run requests in order: Register → Login → Test endpoints

### Option 3: Direct API Calls

Use any HTTP client (curl, fetch, axios, HttpClient, etc.)

**Example with curl:**
```bash
# Register
curl -X POST https://localhost:7186/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!",
    "fullName": "Test User",
    "role": 0
  }'

# Login
curl -X POST https://localhost:7186/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'

# Get User (with token)
curl -X GET https://localhost:7186/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 🔐 Authentication Flow | تدفق المصادقة

```
┌─────────────────────────────────────────────────────────────┐
│ Tafsilk API Auth Flow        │
└─────────────────────────────────────────────────────────────┘

1. Register       2. Login     3. Use Token
   ↓                ↓   ↓
POST /api/auth/register      POST /api/auth/login      GET /api/auth/me
   ↓          ↓       ↓
{email, password, ...}       {email, password}        Authorization: Bearer {token}
   ↓           ↓      ↓
Success: userId              Success: token            Success: user profile
```

---

## 📋 Available Endpoints | نقاط النهاية المتاحة

### Authentication (Public)

| Endpoint | Method | Auth Required | Description |
|----------|--------|---------------|-------------|
| `/api/auth/register` | POST | ❌ | Register customer |
| `/api/auth/login` | POST | ❌ | Login & get token |
| `/api/auth/token` | POST | ❌ | Legacy token endpoint |

### User Management (Protected)

| Endpoint | Method | Auth Required | Description |
|----------|--------|---------------|-------------|
| `/api/auth/me` | GET | ✅ | Get current user |
| `/api/auth/logout` | POST | ✅ | Logout user |
| `/api/auth/refresh` | POST | ✅ | Refresh token (future) |

---

## 🎯 Key Features | الميزات الرئيسية

### ✅ Implemented
- [x] Customer registration
- [x] JWT token authentication
- [x] User profile retrieval
- [x] Role-based authorization
- [x] Arabic error messages
- [x] Comprehensive validation
- [x] Swagger documentation
- [x] XML comments

### 🚧 Future Enhancements
- [ ] Refresh token implementation
- [ ] Token blacklisting
- [ ] Password reset via API
- [ ] Email verification via API
- [ ] Rate limiting
- [ ] API versioning

---

## 📊 Response Codes | رموز الاستجابة

| Code | Status | Arabic | English |
|------|--------|--------|---------|
| 200 | OK | نجح | Success |
| 400 | Bad Request | طلب غير صالح | Invalid request |
| 401 | Unauthorized | غير مصرح | Not authorized |
| 404 | Not Found | غير موجود | Not found |
| 500 | Server Error | خطأ في الخادم | Server error |

---

## 🔧 Configuration | الإعدادات

### appsettings.json

```json
{
  "Jwt": {
"Key": "YourSecretKeyHere-MinimumHere32Chars",
    "Issuer": "TafsilkPlatform",
    "Audience": "TafsilkPlatformUsers",
    "ExpirationMinutes": 60
  },
  "Features": {
    "EnableGoogleOAuth": false
  }
}
```

### Environment Variables

For production, use environment variables instead of appsettings.json:

```bash
Jwt__Key=YourSecretKey
Jwt__Issuer=TafsilkPlatform
Jwt__Audience=TafsilkPlatformUsers
Jwt__ExpirationMinutes=60
```

---

## 🧪 Testing Scenarios | سيناريوهات الاختبار

### ✅ Positive Tests

1. **Register customer** with valid data
2. **Login** with correct credentials
3. **Access protected endpoint** with valid token
4. **Get user profile** successfully
5. **Logout** successfully

### ❌ Negative Tests

1. **Register** with existing email (expect 400)
2. **Register** with weak password (expect 400)
3. **Register** tailor via API (expect 400)
4. **Login** with wrong password (expect 401)
5. **Access protected endpoint** without token (expect 401)
6. **Access protected endpoint** with expired token (expect 401)

### 🔄 Edge Cases

1. Empty request body
2. Null values
3. SQL injection attempts
4. XSS attempts
5. Very long strings
6. Special characters in passwords
7. International phone numbers

---

## 📱 Integration Examples | أمثلة التكامل

### JavaScript (React/Vue/Angular)

```javascript
// Register
const register = async (userData) => {
  const response = await fetch('https://localhost:7186/api/auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify(userData)
  });
  return response.json();
};

// Login
const login = async (email, password) => {
  const response = await fetch('https://localhost:7186/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password, rememberMe: false })
  });
  const data = await response.json();
  
  if (data.token) {
    localStorage.setItem('token', data.token);
  }
  
  return data;
};

// Get current user
const getCurrentUser = async () => {
  const token = localStorage.getItem('token');
  const response = await fetch('https://localhost:7186/api/auth/me', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
return response.json();
};
```

### C# (.NET/Xamarin/MAUI)

```csharp
public class TafsilkApiService
{
    private readonly HttpClient _client;
    
    public TafsilkApiService()
    {
        _client = new HttpClient
        {
       BaseAddress = new Uri("https://localhost:7186")
        };
 }
    
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
      var response = await _client.PostAsJsonAsync("/api/auth/register", request);
  return await response.Content.ReadFromJsonAsync<RegisterResponse>();
    }
    
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new { email, password, rememberMe = false };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
 var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
  
        if (!string.IsNullOrEmpty(result.Token))
        {
            _client.DefaultRequestHeaders.Authorization = 
           new AuthenticationHeaderValue("Bearer", result.Token);
        }
  
        return result;
    }
    
  public async Task<UserProfile> GetCurrentUserAsync()
  {
      var response = await _client.GetAsync("/api/auth/me");
   return await response.Content.ReadFromJsonAsync<UserProfile>();
    }
}
```

### Python (Flask/Django/FastAPI)

```python
import requests

class TafsilkAPI:
    def __init__(self, base_url="https://localhost:7186"):
        self.base_url = base_url
        self.token = None
    
    def register(self, email, password, full_name, role=0):
  response = requests.post(
 f"{self.base_url}/api/auth/register",
        json={
                "email": email,
       "password": password,
    "fullName": full_name,
      "role": role
            }
        )
        return response.json()
    
    def login(self, email, password):
    response = requests.post(
    f"{self.base_url}/api/auth/login",
    json={
      "email": email,
    "password": password,
     "rememberMe": False
            }
   )
        data = response.json()
        
     if "token" in data:
            self.token = data["token"]
        
    return data
    
    def get_current_user(self):
     headers = {"Authorization": f"Bearer {self.token}"}
        response = requests.get(
      f"{self.base_url}/api/auth/me",
            headers=headers
        )
        return response.json()
```

---

## 🛡️ Security Best Practices | أفضل ممارسات الأمان

### For Developers

1. **Always use HTTPS** in production
2. **Store tokens securely** (not in localStorage for sensitive apps)
3. **Implement token refresh** before expiration
4. **Validate all inputs** on client side too
5. **Handle errors gracefully**
6. **Log security events**
7. **Use environment variables** for secrets

### For API Users

1. **Never commit tokens** to version control
2. **Rotate tokens regularly**
3. **Use strong passwords** (min 6 chars, alphanumeric + symbols)
4. **Implement retry logic** with exponential backoff
5. **Monitor API usage** and rate limits
6. **Report security issues** responsibly

---

## 📞 Support & Contact | الدعم والتواصل

### Issues & Bugs
Report issues on GitHub:
- **Repository**: https://github.com/AhmedTyson/Tafsilk
- **Issues**: https://github.com/AhmedTyson/Tafsilk/issues

### Email Support
- **General**: support@tafsilk.com
- **Technical**: dev@tafsilk.com
- **Security**: security@tafsilk.com

### Documentation Updates
This documentation is maintained alongside the codebase. For updates or corrections, please submit a pull request.

---

## 📜 License | الترخيص

Copyright © 2025 Tafsilk Platform
All rights reserved.

Use under Tafsilk License
https://tafsilk.com/license

---

## 🔄 Version History | تاريخ الإصدارات

### v1.0.0 (January 2025)
- ✅ Initial API release
- ✅ JWT authentication
- ✅ Customer registration
- ✅ User profile management
- ✅ Swagger documentation
- ✅ Postman collection

### Upcoming in v1.1.0
- 🚧 Refresh token implementation
- 🚧 Password reset API
- 🚧 Email verification API
- 🚧 Rate limiting

---

## 🎓 Learning Resources | موارد التعلم

### Official Documentation
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core)
- [JWT.io](https://jwt.io)
- [Swagger/OpenAPI](https://swagger.io/docs/)

### Tutorials
- [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication)
- [Swagger in ASP.NET Core](https://learn.microsoft.com/aspnet/core/tutorials/web-api-help-pages-using-swagger)
- [RESTful API Design](https://restfulapi.net/)

---

**Last Updated**: January 2025  
**Maintained by**: Tafsilk Development Team  
**Version**: 1.0.0

