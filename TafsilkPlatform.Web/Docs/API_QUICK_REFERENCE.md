# Tafsilk Platform API - Quick Reference
# مرجع سريع لـ Tafsilk Platform API

## 🚀 Quick Start

### 1. Access Swagger
```
https://localhost:7186/swagger
```

### 2. Test Flow
```
Register → Login → Authorize → Test Endpoints
```

---

## 📌 API Endpoints Summary

### Authentication Endpoints (No Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new customer |
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/token` | Legacy token generation |

### Protected Endpoints (Requires Auth)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/auth/me` | Get current user info |
| POST | `/api/auth/logout` | Logout current user |

---

## 🔑 Authentication Header Format

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📋 Sample Requests

### Register Customer
```json
POST /api/auth/register
{
  "email": "test@example.com",
  "password": "SecurePass123!",
  "fullName": "Test User",
  "phoneNumber": "+966512345678",
  "role": 0
}
```

### Login
```json
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

### Get User Profile
```http
GET /api/auth/me
Authorization: Bearer {token}
```

---

## ⚠️ Common Errors

| Status | Arabic Message | Reason |
|--------|---------------|---------|
| 400 | البريد الإلكتروني مسجل بالفعل | Email already exists |
| 400 | كلمة المرور يجب أن تكون 6 أحرف على الأقل | Password too short |
| 401 | البريد الإلكتروني أو كلمة المرور غير صحيحة | Invalid credentials |
| 401 | جلسة غير صالحة | Invalid/expired token |

---

## 🎯 Role Values

- `0` = Customer (العميل)
- `1` = Tailor (الخياط) - Web only

---

## ⏰ Token Lifetime

- **Default Expiration**: 60 minutes
- **Configurable**: Via `appsettings.json`
- **Format**: JWT (JSON Web Token)

---

## 📊 Response Structure

### Success Response
```json
{
  "success": true,
  "message": "عملية ناجحة",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "خطأ في العملية",
  "error": "ERROR_CODE"
}
```

---

## 🔧 Swagger Authorization Steps

1. Click **Authorize** 🔓 button (top right)
2. Enter: `Bearer {your-token}`
3. Click **Authorize** button
4. Click **Close**
5. Test protected endpoints

---

## 📦 Import to Postman

1. Open Postman
2. Click **Import**
3. Select `Tafsilk_API.postman_collection.json`
4. Collection ready to use!

---

## 🌐 Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `baseUrl` | API base URL | `https://localhost:7186` |
| `token` | JWT token | Auto-set after login |
| `userId` | Current user ID | Auto-set after login |

---

## 📱 Mobile/SPA Integration

### Setup Axios (JavaScript)
```javascript
const api = axios.create({
  baseURL: 'https://localhost:7186',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add token to all requests
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

### Setup HttpClient (C#/.NET)
```csharp
var client = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7186")
};

// Add token
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

---

## 🧪 Test Checklist

- [ ] Register new customer
- [ ] Login and receive token
- [ ] Authorize in Swagger
- [ ] Get user profile
- [ ] Test with expired token
- [ ] Test with invalid credentials
- [ ] Logout

---

## 📖 Full Documentation

- **Swagger UI**: `/swagger`
- **Swagger JSON**: `/swagger/v1/swagger.json`
- **Detailed Guide**: See `SWAGGER_TESTING_GUIDE.md`

---

**Version**: 1.0.0 | **Updated**: January 2025
