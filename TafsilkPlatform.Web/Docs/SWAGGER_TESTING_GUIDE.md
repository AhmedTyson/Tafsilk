# Tafsilk Platform API - Swagger Testing Guide
# دليل اختبار Tafsilk Platform API باستخدام Swagger

## 📋 Table of Contents | جدول المحتويات

1. [Accessing Swagger UI](#accessing-swagger-ui)
2. [Authentication Setup](#authentication-setup)
3. [API Endpoints Testing Examples](#api-endpoints-testing-examples)
4. [Common Response Codes](#common-response-codes)
5. [Troubleshooting](#troubleshooting)

---

## 🌐 Accessing Swagger UI | الوصول إلى واجهة Swagger

### Development Environment | بيئة التطوير

Swagger UI is only available in development mode:

```
https://localhost:7186/swagger
http://localhost:5140/swagger
```

### Features | الميزات

- **Interactive API Documentation**: Test all endpoints directly from the browser
- **Request/Response Examples**: See sample data for all operations
- **Schema Information**: View all model definitions and validations
- **Try It Out**: Execute real API calls with custom parameters

---

## 🔐 Authentication Setup | إعداد المصادقة

### Step 1: Register a New User | تسجيل مستخدم جديد

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "test.customer@example.com",
  "password": "SecurePass123!",
  "fullName": "Test Customer",
  "phoneNumber": "+966512345678",
  "role": 0
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "تم إنشاء الحساب بنجاح. يرجى تسجيل الدخول",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Step 2: Login to Get JWT Token | تسجيل الدخول للحصول على رمز JWT

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "test.customer@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
"message": "تم تسجيل الدخول بنجاح",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-03T12:00:00Z",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "test.customer@example.com",
    "role": "Customer",
    "isActive": true
  }
}
```

### Step 3: Authorize in Swagger | التفويض في Swagger

1. **Click the "Authorize" button** (🔓 lock icon) at the top right of Swagger UI
2. **Enter the token** in this format:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. **Click "Authorize"** then **"Close"**
4. Now all protected endpoints will include the token automatically

---

## 🧪 API Endpoints Testing Examples | أمثلة اختبار نقاط النهاية

### 1. Register Customer (No Auth Required) | تسجيل عميل (لا يتطلب مصادقة)

**Endpoint:** `POST /api/auth/register`

**Test Case 1: Successful Registration**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "MyPassword123!",
  "fullName": "Ahmed Mohamed Ali",
  "phoneNumber": "+966512345678",
  "role": 0
}
```

**Expected Result:** 
- Status: `200 OK`
- Response includes `success: true` and `userId`

**Test Case 2: Duplicate Email**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "MyPassword123!",
  "fullName": "Another User",
  "role": 0
}
```

**Expected Result:**
- Status: `400 Bad Request`
- Response: `"البريد الإلكتروني مسجل بالفعل"`

**Test Case 3: Tailor Registration via API (Should Fail)**
```json
{
  "email": "tailor@example.com",
  "password": "TailorPass123!",
  "fullName": "Master Tailor",
  "shopName": "The Best Tailor Shop",
  "city": "Riyadh",
  "role": 1
}
```

**Expected Result:**
- Status: `400 Bad Request`
- Message: `"تسجيل الخياطين يجب أن يتم عبر الموقع لتقديم الأوراق الثبوتية"`
- Includes redirect URL

---

### 2. Login (No Auth Required) | تسجيل الدخول (لا يتطلب مصادقة)

**Endpoint:** `POST /api/auth/login`

**Test Case 1: Successful Login**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "MyPassword123!",
  "rememberMe": false
}
```

**Expected Result:**
- Status: `200 OK`
- Response includes `token`, `expiresAt`, and user information

**Test Case 2: Invalid Credentials**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "WrongPassword",
  "rememberMe": false
}
```

**Expected Result:**
- Status: `401 Unauthorized`
- Message: `"البريد الإلكتروني أو كلمة المرور غير صحيحة"`

**Test Case 3: Inactive Account**
```json
{
"email": "inactive.user@example.com",
  "password": "ValidPassword123!",
  "rememberMe": false
}
```

**Expected Result:**
- Status: `401 Unauthorized`
- Message: `"حسابك غير نشط. يرجى الاتصال بالدعم"`
- Response includes `isPending: true`

---

### 3. Get Current User (Auth Required) | الحصول على المستخدم الحالي (يتطلب مصادقة)

**Endpoint:** `GET /api/auth/me`

**Prerequisites:** 
- Must be authenticated (click Authorize and add Bearer token)

**Test Case 1: Valid Token**

No request body needed. Just click "Try it out" then "Execute".

**Expected Result for Customer:**
```json
{
  "success": true,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "ahmed.mohamed@example.com",
    "phoneNumber": "+966512345678",
    "role": "customer",
    "isActive": true,
    "createdAt": "2025-01-01T10:00:00Z",
    "profile": {
      "fullName": "Ahmed Mohamed Ali",
      "city": "Riyadh",
      "gender": "Male",
      "dateOfBirth": "1990-01-01T00:00:00Z"
    }
  }
}
```

**Expected Result for Tailor:**
```json
{
  "success": true,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "tailor@example.com",
    "phoneNumber": "+966512345678",
    "role": "tailor",
    "isActive": true,
    "createdAt": "2025-01-01T10:00:00Z",
    "profile": {
      "fullName": "Mohammed Ali",
   "shopName": "Master Tailor Shop",
      "city": "Jeddah",
      "isVerified": true,
      "averageRating": 4.5,
      "experienceYears": 10
    }
  }
}
```

**Test Case 2: Expired/Invalid Token**

Remove or use an invalid token.

**Expected Result:**
- Status: `401 Unauthorized`
- Message: `"جلسة غير صالحة. يرجى تسجيل الدخول مرة أخرى"`

---

### 4. Logout (Auth Required) | تسجيل الخروج (يتطلب مصادقة)

**Endpoint:** `POST /api/auth/logout`

**Prerequisites:**
- Must be authenticated

**Test Case: Successful Logout**

No request body needed.

**Expected Result:**
```json
{
  "success": true,
  "message": "تم تسجيل الخروج بنجاح"
}
```

---

### 5. Legacy Token Endpoint | نقطة الرمز القديمة

**Endpoint:** `POST /api/auth/token`

**Test Case: Generate Simple Token**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "MyPassword123!"
}
```

**Expected Result:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Note:** This endpoint returns only the token string and does not check account status. Use `/api/auth/login` for full authentication.

---

## 📊 Common Response Codes | رموز الاستجابة الشائعة

| Status Code | Meaning | Example Scenario |
|-------------|---------|------------------|
| **200 OK** | Success | Login successful, user data retrieved |
| **400 Bad Request** | Invalid data | Missing required fields, invalid email format |
| **401 Unauthorized** | Authentication failed | Wrong password, expired token |
| **404 Not Found** | Resource not found | User doesn't exist |
| **409 Conflict** | Resource conflict | Email already registered |
| **500 Internal Server Error** | Server error | Database connection failed |

### Arabic Error Messages | رسائل الخطأ بالعربية

| Error Code | Arabic Message | English Translation |
|------------|----------------|---------------------|
| EMAIL_ALREADY_EXISTS | البريد الإلكتروني مسجل بالفعل | Email already registered |
| INVALID_CREDENTIALS | البريد الإلكتروني أو كلمة المرور غير صحيحة | Invalid email or password |
| USER_NOT_ACTIVE | حسابك غير نشط. يرجى الاتصال بالدعم | Account inactive. Contact support |
| TAILOR_INCOMPLETE_PROFILE | يجب تقديم الأوراق الثبوتية لإكمال التسجيل | Must provide evidence to complete registration |
| WEAK_PASSWORD | كلمة المرور ضعيفة جداً | Password too weak |

---

## 🔧 Troubleshooting | استكشاف الأخطاء

### Issue: Swagger UI Not Loading

**Solution:**
1. Ensure you're running in Development mode
2. Check that `app.Environment.IsDevelopment()` is true
3. Verify URL: `https://localhost:7186/swagger`

### Issue: Authorization Not Working

**Solution:**
1. Ensure you clicked "Authorize" button
2. Format must be: `Bearer {token}` (with space after Bearer)
3. Token must not be expired (check `expiresAt` from login response)
4. Don't include quotes around the token

### Issue: 401 Unauthorized for Protected Endpoints

**Solution:**
1. Login first to get a valid token
2. Click Authorize and enter token
3. Ensure token hasn't expired (default: 60 minutes)
4. Check that your account is active

### Issue: XML Documentation Not Showing

**Solution:**
1. Verify `GenerateDocumentationFile` is `true` in `.csproj`
2. Rebuild the project
3. Check that XML file exists in `bin/Debug/net9.0/`

---

## 🎯 Testing Workflow | سير عمل الاختبار

### Complete Test Scenario | سيناريو اختبار كامل

1. **Register a new customer**
   ```
   POST /api/auth/register
   ✅ Expect: 200 OK with userId
   ```

2. **Login with the new account**
   ```
   POST /api/auth/login
   ✅ Expect: 200 OK with token
   ```

3. **Authorize in Swagger**
   ```
   Click Authorize → Enter: Bearer {token}
   ```

4. **Get current user info**
 ```
   GET /api/auth/me
   ✅ Expect: 200 OK with user profile
   ```

5. **Try accessing without token**
   ```
   Click Authorize → Click Logout
   GET /api/auth/me
   ✅ Expect: 401 Unauthorized
   ```

6. **Logout**
   ```
   Re-authorize with token
   POST /api/auth/logout
   ✅ Expect: 200 OK with success message
   ```

---

## 📝 Best Practices | أفضل الممارسات

### For Developers | للمطورين

1. **Always test both success and failure scenarios**
2. **Verify response status codes match documentation**
3. **Check Arabic error messages for user-facing applications**
4. **Test token expiration handling**
5. **Validate all input edge cases**

### For Testers | للمختبرين

1. **Follow the testing workflow sequence**
2. **Document any inconsistencies**
3. **Test with multiple user roles**
4. **Verify all validation messages**
5. **Check API performance and response times**

---

## 🔗 Additional Resources | موارد إضافية

- **Swagger JSON**: `https://localhost:7186/swagger/v1/swagger.json`
- **Project Repository**: [GitHub - Tafsilk Platform](https://github.com/AhmedTyson/Tafsilk)
- **ASP.NET Core Documentation**: https://learn.microsoft.com/aspnet/core
- **JWT.io Token Debugger**: https://jwt.io

---

## 📞 Support | الدعم

For issues or questions:
- **Email**: support@tafsilk.com
- **GitHub Issues**: https://github.com/AhmedTyson/Tafsilk/issues

---

**Last Updated**: January 2025
**Version**: 1.0.0
**Platform**: .NET 9.0

