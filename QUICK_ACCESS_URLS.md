# 🚀 TAFSILK PLATFORM - QUICK ACCESS URLs

## **✅ YOUR APPLICATION IS RUNNING!**

```
Now listening on: http://localhost:5140
```

---

## **📋 ACCESS YOUR API DOCUMENTATION:**

### **🟣 Scalar API Documentation (Modern, Recommended)**

**HTTP URL:**
```
http://localhost:5140/scalar/v1
```

**Features:**
- ✨ Beautiful purple theme
- 🌙 Dark mode
- 💻 C# code examples
- ⚡ Fast and responsive
- 🔐 JWT authentication support

---

### **🔷 Swagger UI (Traditional)**

**HTTP URL:**
```
http://localhost:5140/swagger
```

**Features:**
- 📋 Standard OpenAPI interface
- 🧪 Interactive testing
- 📊 Schema exploration

---

### **📄 Swagger JSON (OpenAPI Spec)**

**HTTP URL:**
```
http://localhost:5140/swagger/v1/swagger.json
```

**Use this for:**
- Importing to Postman
- Code generation tools
- API documentation tools

---

## **🎯 QUICK TEST:**

### **Option 1: Open Scalar (Recommended)**
```
http://localhost:5140/scalar/v1
```
1. Click the URL above or paste in browser
2. See your beautiful API documentation!
3. Browse endpoints in sidebar
4. Test any endpoint with "Try it out"

---

### **Option 2: Open Swagger**
```
http://localhost:5140/swagger
```
1. Click the URL above or paste in browser
2. See traditional Swagger UI
3. Test endpoints interactively

---

## **🔐 TEST AUTHENTICATION:**

### **Step 1: Register a User**

**Endpoint:** `POST /api/auth/register`

**In Scalar or Swagger:**
1. Find the `/api/auth/register` endpoint
2. Click "Try it out"
3. Use this test data:
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "fullName": "Test User",
  "phoneNumber": "+966501234567",
  "role": 0
}
```
4. Click "Execute"
5. Should see: `"success": true`

---

### **Step 2: Login and Get Token**

**Endpoint:** `POST /api/auth/login`

1. Find the `/api/auth/login` endpoint
2. Click "Try it out"
3. Use this test data:
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "rememberMe": false
}
```
4. Click "Execute"
5. **Copy the `token` from the response!**

Example token:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlRlc3QgVXNlciIsImlhdCI6MTUxNjIzOTAyMn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

---

### **Step 3: Authorize**

**In Scalar:**
1. Click **🔒 "Authorize"** button (top right)
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"
4. Close popup

**In Swagger:**
1. Click **"Authorize"** button (🔒 icon)
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click "Authorize"
4. Close popup

---

### **Step 4: Test Protected Endpoint**

**Endpoint:** `GET /api/auth/me`

1. Find the `/api/auth/me` endpoint
2. Click "Try it out"
3. Click "Execute"
4. Should see your user profile data!

---

## **📱 CURL COMMANDS FOR TESTING:**

### **Register:**
```bash
curl -X POST "http://localhost:5140/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "fullName": "Test User",
    "phoneNumber": "+966501234567",
    "role": 0
  }'
```

### **Login:**
```bash
curl -X POST "http://localhost:5140/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "rememberMe": false
  }'
```

### **Get User (with token):**
```bash
curl -X GET "http://localhost:5140/api/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## **💻 POWERSHELL COMMANDS:**

### **Register:**
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
    fullName = "Test User"
    phoneNumber = "+966501234567"
  role = 0
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5140/api/auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### **Login:**
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
    rememberMe = $false
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5140/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$token = $response.token
Write-Host "Token: $token"
```

### **Get User:**
```powershell
$headers = @{
    Authorization = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5140/api/auth/me" `
    -Method GET `
    -Headers $headers
```

---

## **🎨 WHICH SHOULD YOU USE?**

### **🟣 Scalar (Recommended for Development)**
```
http://localhost:5140/scalar/v1
```
**Best for:**
- ✅ Modern, beautiful UI
- ✅ Better developer experience
- ✅ Multiple code examples (C#, JS, Python, etc.)
- ✅ Fast and responsive
- ✅ Dark mode

### **🔷 Swagger (Good for Compatibility)**
```
http://localhost:5140/swagger
```
**Best for:**
- ✅ Standard OpenAPI compliance
- ✅ Familiar to many developers
- ✅ Tool compatibility

**Recommendation:** Try **Scalar first** - it's much nicer! But both work great.

---

## **⚠️ IMPORTANT NOTES:**

### **HTTPS vs HTTP:**

**Your app is currently running on HTTP only:**
- ✅ HTTP: `http://localhost:5140`
- ❌ HTTPS: Not available (port 7186 not listening)

**To enable HTTPS:**
1. Check your launchSettings.json
2. Make sure `applicationUrl` includes HTTPS
3. Trust the dev certificate: `dotnet dev-certs https --trust`

**Current launchSettings.json should have:**
```json
"applicationUrl": "https://localhost:7186;http://localhost:5140"
```

---

## **🔍 TROUBLESHOOTING:**

### **If Scalar/Swagger not loading:**

1. **Check URL is correct:**
   - ✅ `http://localhost:5140/scalar/v1`
   - ❌ `http://localhost:5140/scalar` (missing /v1)

2. **Verify Environment:**
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT
   # Should show: Development
   ```

3. **Check Console Output:**
   Look for these messages:
   ```
   🟣 Scalar API Docs available at: http://localhost:5140/scalar/v1
   🔷 Swagger UI available at: http://localhost:5140/swagger
```

4. **Test Swagger JSON first:**
   ```
   http://localhost:5140/swagger/v1/swagger.json
   ```
   If this works, the API docs are working!

---

## **✅ QUICK CHECKLIST:**

- [ ] Application running on port 5140
- [ ] Environment is Development
- [ ] Open browser to `http://localhost:5140/scalar/v1`
- [ ] See beautiful purple Scalar interface
- [ ] Browse API endpoints in sidebar
- [ ] Test registration endpoint
- [ ] Test login endpoint
- [ ] Get JWT token
- [ ] Authorize with token
- [ ] Test protected endpoint (`/api/auth/me`)

---

## **🎊 SUMMARY:**

**Your Application is Running:**
```
http://localhost:5140
```

**Access API Documentation:**
- 🟣 **Scalar:** `http://localhost:5140/scalar/v1` ← **Try this first!**
- 🔷 **Swagger:** `http://localhost:5140/swagger`
- 📄 **JSON:** `http://localhost:5140/swagger/v1/swagger.json`

**Test Your API:**
1. Open Scalar or Swagger
2. Try `/api/auth/register`
3. Try `/api/auth/login` and get token
4. Authorize with token
5. Try `/api/auth/me`

**Status:** ✅ **READY TO TEST!**

---

**Date:** 2025-01-20  
**Port:** HTTP 5140  
**Next:** Open `http://localhost:5140/scalar/v1` in your browser!

---

**🎉 Start testing your beautiful API documentation!**

**تفصيلك - نربط بينك وبين أفضل الخياطين** 🧵✂️
