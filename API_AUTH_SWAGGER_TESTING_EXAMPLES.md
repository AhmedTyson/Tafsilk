# 🔐 API AUTH CONTROLLER - SWAGGER TESTING EXAMPLES

## **📋 Complete Testing Guide for `/api/auth` Endpoints**

This guide provides **realistic test data** for all authentication endpoints in your Tafsilk Platform API.

---

## **🎯 QUICK ACCESS**

**Swagger UI:** `https://localhost:7186/swagger`

**Base URL:** `https://localhost:7186/api/auth`

---

## **1️⃣ POST /api/auth/register - Register New User**

### **✅ Example 1: Register Customer (Success)**

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "sarah.ahmed@gmail.com",
  "password": "SecurePass123!",
  "fullName": "Sarah Ahmed",
  "phoneNumber": "+966501234567",
  "role": 0,
  "shopName": null,
  "address": null,
  "city": null
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

---

### **✅ Example 2: Register Customer with Optional Phone**

```json
{
  "email": "mohammed.ali@outlook.com",
  "password": "MyPassword456!",
  "fullName": "Mohammed Ali",
  "phoneNumber": "+966555123456",
  "role": 0
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "تم إنشاء الحساب بنجاح. يرجى تسجيل الدخول",
  "userId": "7b8c9d3e-4f5a-6b7c-8d9e-0f1a2b3c4d5e"
}
```

---

### **❌ Example 3: Try to Register Tailor (Should Fail)**

**⚠️ Tailors CANNOT register via API - must use web interface**

```json
{
  "email": "tailor@example.com",
  "password": "TailorPass123!",
  "fullName": "Ahmed Tailor",
  "phoneNumber": "+966501111111",
  "role": 1,
  "shopName": "Ahmed's Tailoring",
  "address": "Riyadh",
  "city": "Riyadh"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "تسجيل الخياطين يجب أن يتم عبر الموقع لتقديم الأوراق الثبوتية",
  "redirectUrl": "/Account/Register"
}
```

---

### **❌ Example 4: Invalid Email Format**

```json
{
  "email": "invalidemail",
  "password": "SecurePass123!",
  "fullName": "Test User",
  "phoneNumber": "+966501234567",
  "role": 0
}
```

**Expected Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "بيانات التسجيل غير صالحة",
  "errors": [
    "البريد الإلكتروني غير صالح"
  ]
}
```

---

### **❌ Example 5: Duplicate Email**

```json
{
  "email": "sarah.ahmed@gmail.com",
  "password": "AnotherPass789!",
  "fullName": "Another Sarah",
  "phoneNumber": "+966509876543",
  "role": 0
}
```

**Expected Response (400 Bad Request):**
```json
{
"success": false,
  "message": "البريد الإلكتروني مسجل بالفعل",
  "error": "EMAIL_ALREADY_EXISTS"
}
```

---

## **2️⃣ POST /api/auth/login - Login and Get JWT Token**

### **✅ Example 1: Successful Login**

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "sarah.ahmed@gmail.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تسجيل الدخول بنجاح",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ1bmlxdWVfbmFtZSI6InNhcmFoLmFobWVkQGdtYWlsLmNvbSIsImVtYWlsIjoic2FyYWguYWhtZWRAZ21haWwuY29tIiwicm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzA2MjA0NDAwfQ.a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2",
  "expiresAt": "2025-01-20T18:00:00Z",
  "user": {
 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "sarah.ahmed@gmail.com",
    "role": "Customer",
    "isActive": true
  }
}
```

---

### **✅ Example 2: Login with Remember Me**

```json
{
  "email": "mohammed.ali@outlook.com",
  "password": "MyPassword456!",
  "rememberMe": true
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تسجيل الدخول بنجاح",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-20T18:00:00Z",
  "user": {
    "id": "7b8c9d3e-4f5a-6b7c-8d9e-0f1a2b3c4d5e",
    "email": "mohammed.ali@outlook.com",
    "role": "Customer",
    "isActive": true
  }
}
```

---

### **❌ Example 3: Invalid Credentials**

```json
{
  "email": "sarah.ahmed@gmail.com",
  "password": "WrongPassword123!",
  "rememberMe": false
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "البريد الإلكتروني أو كلمة المرور غير صحيحة",
  "error": "INVALID_CREDENTIALS"
}
```

---

### **❌ Example 4: Inactive Account**

```json
{
  "email": "inactive.user@gmail.com",
  "password": "ValidPass123!",
  "rememberMe": false
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "حسابك غير نشط. يرجى الاتصال بالدعم",
  "isPending": false,
  "role": "customer"
}
```

---

### **❌ Example 5: Tailor with Incomplete Profile**

```json
{
  "email": "tailor.incomplete@gmail.com",
  "password": "TailorPass123!",
  "rememberMe": false
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "يجب تقديم الأوراق الثبوتية لإكمال التسجيل قبل تسجيل الدخول",
  "requiresEvidence": true,
  "redirectUrl": "/Account/CompleteTailorProfile",
  "userId": "9c8d7e6f-5a4b-3c2d-1e0f-9a8b7c6d5e4f"
}
```

---

## **3️⃣ GET /api/auth/me - Get Current User Info**

### **✅ Example: Get Authenticated User (Customer)**

**Endpoint:** `GET /api/auth/me`

**Authorization Required:** `Bearer {your_jwt_token}`

**How to Use in Swagger:**
1. First, login using `/api/auth/login` to get a token
2. Click the **"Authorize"** button (🔒) at top right
3. Enter: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. Click **"Authorize"**
5. Now call `/api/auth/me`

**Request:** (No body, just headers with JWT token)

**Expected Response (200 OK) - Customer:**
```json
{
  "success": true,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "sarah.ahmed@gmail.com",
    "phoneNumber": "+966501234567",
    "role": "customer",
    "isActive": true,
    "createdAt": "2025-01-15T10:30:00Z",
    "profile": {
      "fullName": "Sarah Ahmed",
      "city": "Riyadh",
 "gender": "Female",
      "dateOfBirth": "1995-05-15T00:00:00Z"
    }
  }
}
```

---

### **✅ Example: Get Authenticated User (Tailor)**

**Expected Response (200 OK) - Tailor:**
```json
{
  "success": true,
  "user": {
    "id": "9c8d7e6f-5a4b-3c2d-1e0f-9a8b7c6d5e4f",
    "email": "tailor.verified@gmail.com",
    "phoneNumber": "+966502222222",
    "role": "tailor",
    "isActive": true,
    "createdAt": "2025-01-10T08:00:00Z",
  "profile": {
      "fullName": "Ahmed Al-Tailor",
      "shopName": "Ahmed's Professional Tailoring",
      "city": "Jeddah",
      "isVerified": true,
      "averageRating": 4.75,
      "experienceYears": 10
    }
  }
}
```

---

### **❌ Example: Unauthorized (No Token)**

**Request:** Call without Authorization header

**Expected Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "جلسة غير صالحة. يرجى تسجيل الدخول مرة أخرى"
}
```

---

### **❌ Example: Invalid/Expired Token**

**Request:** Call with expired or invalid JWT token

**Expected Response (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

---

## **4️⃣ POST /api/auth/refresh - Refresh JWT Token**

### **⚠️ Currently Not Implemented**

**Endpoint:** `POST /api/auth/refresh`

**Request Body:**
```json
{
  "refreshToken": "your_refresh_token_here"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "تحديث الرمز غير مدعوم حالياً"
}
```

**Note:** This endpoint is reserved for future implementation with refresh token logic.

---

## **5️⃣ POST /api/auth/logout - Logout**

### **✅ Example: Successful Logout**

**Endpoint:** `POST /api/auth/logout`

**Authorization Required:** `Bearer {your_jwt_token}`

**Request:** (No body)

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "تم تسجيل الخروج بنجاح"
}
```

**Note:** Currently, logout is mainly client-side (delete the token). Server-side token blacklisting is not yet implemented.

---

## **📊 SWAGGER UI TESTING WORKFLOW**

### **Complete Test Scenario:**

#### **Step 1: Register a New Customer**
1. Open Swagger: `https://localhost:7186/swagger`
2. Find **POST /api/auth/register**
3. Click **"Try it out"**
4. Paste this:
```json
{
  "email": "test.user@gmail.com",
  "password": "TestPass123!",
  "fullName": "Test User",
  "phoneNumber": "+966501234567",
  "role": 0
}
```
5. Click **"Execute"**
6. Should see: `"success": true`

---

#### **Step 2: Login and Get JWT Token**
1. Find **POST /api/auth/login**
2. Click **"Try it out"**
3. Paste this:
```json
{
  "email": "test.user@gmail.com",
  "password": "TestPass123!",
  "rememberMe": false
}
```
4. Click **"Execute"**
5. **Copy the `token` from the response!**

Example token:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ1bmlxdWVfbmFtZSI6InRlc3QudXNlckBnbWFpbC5jb20iLCJlbWFpbCI6InRlc3QudXNlckBnbWFpbC5jb20iLCJyb2xlIjoiQ3VzdG9tZXIiLCJleHAiOjE3MDYyMDQ0MDB9.a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2
```

---

#### **Step 3: Authorize in Swagger**
1. Click the **"Authorize"** button (🔒 green lock icon at top right)
2. In the popup, enter:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ1bmlxdWVfbmFtZSI6InRlc3QudXNlckBnbWFpbC5jb20iLCJlbWFpbCI6InRlc3QudXNlckBnbWFpbC5jb20iLCJyb2xlIjoiQ3VzdG9tZXIiLCJleHAiOjE3MDYyMDQ0MDB9.a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2
```
3. Click **"Authorize"**
4. Close the popup
5. Notice the 🔒 locks on protected endpoints turn green

---

#### **Step 4: Get Current User Info**
1. Find **GET /api/auth/me**
2. Notice it now has a green 🔒 (authorized)
3. Click **"Try it out"**
4. Click **"Execute"**
5. Should see your user profile data!

---

#### **Step 5: Logout**
1. Find **POST /api/auth/logout**
2. Click **"Try it out"**
3. Click **"Execute"**
4. Should see: `"success": true`
5. Delete the token from your client

---

## **🔑 ROLE ENUM VALUES**

When using the `role` field in registration:

```csharp
public enum RegistrationRole
{
    Customer = 0,  // ← Use 0 for customers
    Tailor = 1     // ← Use 1 for tailors (blocked in API)
}
```

**Valid Values:**
- `0` = Customer ✅
- `1` = Tailor ❌ (must register via web)

---

## **🎯 COMMON TEST SCENARIOS**

### **Scenario 1: Complete User Journey**
```
1. Register new customer → Success
2. Login with credentials → Get JWT token
3. Authorize in Swagger → Add Bearer token
4. Get user info → See profile data
5. Logout → Success
```

---

### **Scenario 2: Error Handling**
```
1. Try to register with invalid email → 400 error
2. Try to register duplicate email → 400 error
3. Try to login with wrong password → 401 error
4. Try to access /me without token → 401 error
5. Try to register tailor via API → 400 error
```

---

### **Scenario 3: Token Expiration**
```
1. Login → Get token (expires in 60 minutes)
2. Wait 60 minutes
3. Try to call /me → 401 Unauthorized
4. Login again → Get new token
5. Use new token → Success
```

---

## **📱 TESTING FROM OTHER TOOLS**

### **Postman:**

**Register Request:**
```http
POST https://localhost:7186/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "TestPass123!",
  "fullName": "Test User",
  "phoneNumber": "+966501234567",
  "role": 0
}
```

**Login Request:**
```http
POST https://localhost:7186/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "TestPass123!",
  "rememberMe": false
}
```

**Get User Info (Authenticated):**
```http
GET https://localhost:7186/api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### **cURL:**

**Register:**
```bash
curl -X POST "https://localhost:7186/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
 "password": "TestPass123!",
    "fullName": "Test User",
    "phoneNumber": "+966501234567",
    "role": 0
  }'
```

**Login:**
```bash
curl -X POST "https://localhost:7186/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPass123!",
    "rememberMe": false
  }'
```

**Get User (Authenticated):**
```bash
curl -X GET "https://localhost:7186/api/auth/me" \
-H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### **PowerShell:**

**Register:**
```powershell
$body = @{
    email = "test@example.com"
    password = "TestPass123!"
    fullName = "Test User"
    phoneNumber = "+966501234567"
role = 0
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7186/api/auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Login:**
```powershell
$body = @{
    email = "test@example.com"
    password = "TestPass123!"
    rememberMe = $false
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7186/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$token = $response.token
Write-Host "Token: $token"
```

**Get User (Authenticated):**
```powershell
$headers = @{
    Authorization = "Bearer $token"
}

Invoke-RestMethod -Uri "https://localhost:7186/api/auth/me" `
    -Method GET `
    -Headers $headers
```

---

## **🎨 JWT TOKEN STRUCTURE**

Your JWT tokens include these claims:

```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "unique_name": "test@example.com",
"email": "test@example.com",
  "role": "Customer",
  "exp": 1706204400,
  "iss": "TafsilkPlatform",
  "aud": "TafsilkPlatformUsers"
}
```

**Decode your token at:** https://jwt.io

---

## **✅ VALIDATION RULES**

### **Email:**
- ✅ Required
- ✅ Must be valid email format
- ✅ Max 255 characters
- ✅ Must be unique

### **Password:**
- ✅ Required
- ✅ Minimum 6 characters (basic validation)
- ⚠️ Consider: uppercase, lowercase, digit, special char

### **Full Name:**
- ✅ Required
- ✅ Max 255 characters

### **Phone Number:**
- ⚠️ Optional
- ✅ Must be valid phone format if provided

### **Role:**
- ✅ 0 = Customer (allowed via API)
- ❌ 1 = Tailor (blocked via API)

---

## **🎊 SUMMARY**

**Endpoints Available:**
1. ✅ `POST /api/auth/register` - Register new customer
2. ✅ `POST /api/auth/login` - Get JWT token
3. ✅ `GET /api/auth/me` - Get user info (requires auth)
4. ⚠️ `POST /api/auth/refresh` - Not implemented yet
5. ✅ `POST /api/auth/logout` - Logout (client-side mostly)

**Testing Steps:**
1. Register → Login → Get Token → Authorize → Test Protected Endpoints

**Common Test Users:**
```
Email: sarah.ahmed@gmail.com
Password: SecurePass123!
Role: Customer

Email: mohammed.ali@outlook.com
Password: MyPassword456!
Role: Customer
```

**Swagger URL:** `https://localhost:7186/swagger`

---

**Date:** 2025-01-20  
**Status:** Ready to test!  
**Next:** Open Swagger and start testing! 🚀

---

**🎉 Happy Testing!**

**تفصيلك - نربط بينك وبين أفضل الخياطين** 🧵✂️
