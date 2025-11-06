# Visual Guide: Testing Tafsilk API with Swagger
# دليل مرئي: اختبار Tafsilk API باستخدام Swagger

## 📸 Step-by-Step Visual Instructions | تعليمات مرئية خطوة بخطوة

---

## Step 1: Access Swagger UI | الخطوة 1: الوصول إلى Swagger UI

### What to do:
1. Start your application in Development mode
2. Open your browser
3. Navigate to: `https://localhost:7186/swagger`

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ 🌐 Browser Address Bar    │
│ https://localhost:7186/swagger   │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│         │
│    Tafsilk Platform API v1│
│    Tafsilk - منصة الخياطين والتفصيل            │
│             │
│    [ Authorize 🔓 ]           │
│         │
│    ▼ api/auth [2 operations]    │
│    ▼ ApiAuth [5 operations]     │
│         │
└────────────────────────────────────────────────────────────┘
```

**✅ Success Indicator:** You see the Swagger UI page with API documentation

**❌ If you see an error:**
- Verify you're running in Development mode
- Check the application is running
- Try `http://localhost:5140/swagger` if HTTPS doesn't work

---

## Step 2: Explore Endpoints | الخطوة 2: استكشاف نقاط النهاية

### What to do:
1. Scroll down to see all endpoints
2. Click on any endpoint to expand it
3. Read the descriptions and examples

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ POST /api/auth/register     │
│ Register a new user account (Customer only via API)       │
│ تسجيل حساب مستخدم جديد (العملاء فقط عبر API)            │
│       │
│ [Try it out]          │
│            │
│ Sample request:        │
│ {            │
│   "email": "customer@example.com",        │
│   "password": "SecurePassword123!",           │
│   "fullName": "Ahmed Mohamed",          │
│   "phoneNumber": "+966512345678",│
│   "role": 0               │
│ }  │
│    │
│ Parameters:        │
│ • email (required) - User's email address          │
│ • password (required) - User's password        │
│ • fullName (required) - User's full name        │
│ • phoneNumber (optional) - User's phone                  │
│ • role (required) - 0=Customer, 1=Tailor   │
│          │
│ Responses:     │
│ 200 - Success    │
│ 400 - Bad Request      │
└────────────────────────────────────────────────────────────┘
```

**✅ Success Indicator:** Detailed descriptions appear with examples

---

## Step 3: Test Register Endpoint | الخطوة 3: اختبار نقطة التسجيل

### What to do:
1. Click on `POST /api/auth/register`
2. Click the **[Try it out]** button
3. Edit the request body (or use the default)
4. Click **[Execute]** button

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ Request body        │
│ ┌────────────────────────────────────────────────────────┐ │
│ │ {        │ │
│ │   "email": "test.customer@example.com",    │ │
│ │   "password": "SecurePass123!",         │ │
│ │   "fullName": "Test Customer",     │ │
│ │   "phoneNumber": "+966512345678",      │ │
│ │   "role": 0    │ │
│ │ }      │ │
│ └────────────────────────────────────────────────────────┘ │
│    │
│ [ Execute ]  [ Clear ]       │
│   │
│ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│            │
│ Responses       │
│   │
│ ✅ Code: 200          │
│ {            │
│ "success": true,        │
│   "message": "تم إنشاء الحساب بنجاح. يرجى تسجيل الدخول", │
│   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"       │
│ }       │
│ │
│ Response headers    │
│ content-type: application/json; charset=utf-8 │
│    │
│ Curl    │
│ curl -X POST "https://localhost:7186/api/auth/register"  │
│  -H "Content-Type: application/json" │
│      -d "{...}"    │
└────────────────────────────────────────────────────────────┘
```

**✅ Success Indicator:** 
- Code: 200 (green)
- Response shows `"success": true`
- You receive a `userId`

**❌ Common Errors:**
- 400: Email already exists or invalid data
- Check your request body format

---

## Step 4: Test Login Endpoint | الخطوة 4: اختبار نقطة تسجيل الدخول

### What to do:
1. Click on `POST /api/auth/login`
2. Click **[Try it out]**
3. Enter the same email/password from registration
4. Click **[Execute]**
5. **IMPORTANT:** Copy the token from the response

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ Request body      │
│ ┌────────────────────────────────────────────────────────┐ │
│ │ {             │ │
│ │   "email": "test.customer@example.com",   │ │
│ │   "password": "SecurePass123!",              │ │
│ │   "rememberMe": false             │ │
│ │ }     │ │
│ └────────────────────────────────────────────────────────┘ │
│   │
│ [ Execute ]       │
│    │
│ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│        │
│ ✅ Code: 200             │
│ {       │
│   "success": true,│
│   "message": "تم تسجيل الدخول بنجاح",     │
│   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",   │
│   "expiresAt": "2025-01-03T12:00:00Z",          │
│   "user": {                  │
│     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",       │
│     "email": "test.customer@example.com",           │
│     "role": "Customer",        │
│     "isActive": true              │
│}    │
│ }          │
└────────────────────────────────────────────────────────────┘

    👆 COPY THIS TOKEN! You'll need it for the next step.
```

**✅ Success Indicator:**
- Code: 200
- You receive a long `token` string
- User information is returned

---

## Step 5: Authorize in Swagger | الخطوة 5: التفويض في Swagger

### What to do:
1. Find the **[Authorize 🔓]** button at the top right
2. Click it
3. A modal/popup will appear
4. In the "Value" field, type: `Bearer {paste-your-token-here}`
5. Click **[Authorize]**
6. Click **[Close]**

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│            Available authorizations    │
│        │
│  Bearer (http, Bearer) │
│  ┌──────────────────────────────────────────────────────┐ │
│  │ Value:     │ │
│  │ Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...      │ │
│  │            │ │
│  └──────────────────────────────────────────────────────┘ │
│  │
│            [ Authorize ]    [ Close ]          │
│         │
│  ⓘ JWT Authorization header using the Bearer scheme.      │
│     Example: "Bearer {token}"    │
└────────────────────────────────────────────────────────────┘

After clicking Authorize, the button changes:
[ Authorize 🔒 ]  ← Now shows a lock icon
```

**✅ Success Indicator:**
- The lock icon becomes locked 🔒
- No error message appears

**❌ Common Mistakes:**
- Forgetting to type "Bearer " before the token
- Having extra spaces or quotes
- Correct format: `Bearer eyJhbG...` (one space after Bearer)

---

## Step 6: Test Protected Endpoint | الخطوة 6: اختبار نقطة محمية

### What to do:
1. Click on `GET /api/auth/me`
2. Click **[Try it out]**
3. Click **[Execute]** (no body needed)

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ GET /api/auth/me   │
│ Get current authenticated user information     │
│    │
│ 🔒 Requires authorization  │
│     │
│ [ Try it out ]     │
│ [ Execute ]            │
│      │
│ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│     │
│ ✅ Code: 200 │
│ {   │
│   "success": true,    │
│   "user": {         │
│     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", │
│     "email": "test.customer@example.com",          │
│     "phoneNumber": "+966512345678",       │
│"role": "customer",        │
│     "isActive": true,   │
│     "createdAt": "2025-01-01T10:00:00Z",            │
│     "profile": {  │
│       "fullName": "Test Customer",          │
│       "city": null,  │
│       "gender": null,   │
│       "dateOfBirth": null    │
│     }   │
│   }       │
│ }                │
└────────────────────────────────────────────────────────────┘
```

**✅ Success Indicator:**
- Code: 200
- Your user profile data is returned
- All fields are populated correctly

**❌ If you get 401 Unauthorized:**
- Token wasn't added correctly
- Go back to Step 5 and re-authorize
- Make sure the token hasn't expired (60 min default)

---

## Step 7: Test Logout | الخطوة 7: اختبار تسجيل الخروج

### What to do:
1. Click on `POST /api/auth/logout`
2. Click **[Try it out]**
3. Click **[Execute]**

### What you should see:
```
┌────────────────────────────────────────────────────────────┐
│ POST /api/auth/logout             │
│ Logout and invalidate token              │
│              │
│ [ Execute ]         │
│            │
│ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│       │
│ ✅ Code: 200         │
│ { │
│   "success": true,          │
│   "message": "تم تسجيل الخروج بنجاح"      │
│ }          │
└────────────────────────────────────────────────────────────┘
```

**✅ Success Indicator:**
- Code: 200
- Success message in Arabic

---

## 🎯 Complete Test Checklist | قائمة اختبار كاملة

Use this checklist to verify everything works:

- [ ] Step 1: Access Swagger UI successfully
- [ ] Step 2: See all API endpoints listed
- [ ] Step 3: Register a new customer (200 OK)
- [ ] Step 4: Login and receive token (200 OK)
- [ ] Step 5: Authorize with Bearer token
- [ ] Step 6: Get current user profile (200 OK)
- [ ] Step 7: Logout successfully (200 OK)
- [ ] Bonus: Try accessing /me without authorization (401)
- [ ] Bonus: Try registering with same email (400)
- [ ] Bonus: Try login with wrong password (401)

---

## 🔍 Understanding the Interface | فهم الواجهة

### Color Coding in Swagger:

```
🟢 Green (200) = Success
   Everything worked correctly

🟡 Yellow (400) = Bad Request
 Your data is invalid or incomplete

🔴 Red (401) = Unauthorized
   You need to login or provide valid token

🔴 Red (404) = Not Found
   The resource doesn't exist

🔴 Red (500) = Server Error
   Something went wrong on the server
```

### HTTP Methods:

```
POST   = Create new resource
GET    = Retrieve resource
PUT    = Update entire resource
PATCH  = Update partial resource
DELETE = Remove resource
```

### Response Sections:

```
┌─────────────────────────────────┐
│ Server response     │  ← HTTP status code
│ Code: 200        │
│ {response JSON}                 │  ← Response body
│            │
│ Response headers     │  ← HTTP headers
│ content-type: application/json  │
│        │
│ Curl        │  ← Copy-paste command
│ curl -X POST "..."   │
│       │
│ Request URL   │  ← Full endpoint URL
│ https://localhost:7186/api/...  │
└─────────────────────────────────┘
```

---

## 🛠️ Troubleshooting Common Issues | حل المشاكل الشائعة

### Issue 1: "Failed to fetch" Error

**Problem:** Red error message saying "Failed to fetch"

**Solutions:**
```
1. Check if the application is running
   → Open terminal, look for "Now listening on..."
   
2. Verify the URL
   → Should be https://localhost:7186
   
3. Accept SSL certificate
   → Browser may show security warning, click "Advanced" → "Proceed"
```

### Issue 2: 401 Unauthorized for Protected Endpoints

**Problem:** Can't access /api/auth/me

**Solutions:**
```
1. Check authorization
   → Click Authorize button at top
   → Verify Bearer token is present
   
2. Check token format
   → Must be: Bearer {token}
   → One space after "Bearer"
   → No quotes around token
   
3. Check token expiration
   → Default expiry: 60 minutes
   → Login again to get new token
```

### Issue 3: 400 Bad Request on Register

**Problem:** Registration fails with 400

**Common Causes:**
```
❌ Email format invalid
   ✅ Use: user@example.com

❌ Password too short
   ✅ Minimum 6 characters

❌ Email already exists
   ✅ Use different email

❌ Missing required fields
   ✅ Check email, password, fullName are present
```

### Issue 4: XML Comments Not Showing

**Problem:** Swagger doesn't show descriptions

**Solutions:**
```
1. Rebuild the project
   → dotnet build
   
2. Check project file
   → <GenerateDocumentationFile>true</GenerateDocumentationFile>
   
3. Restart application
   → Stop and start again
   
4. Clear browser cache
   → Hard refresh: Ctrl+Shift+R (Windows) or Cmd+Shift+R (Mac)
```

---

## 📱 Mobile/Tablet View | عرض الجوال/التابلت

Swagger UI is responsive and works on mobile devices:

```
Mobile Layout:
┌─────────────┐
│ ☰ Menu      │
│             │
│ Tafsilk API │
│        │
│ ▼ Endpoints │
│   POST ...  │
│   GET ...   │
│             │
│ [Try it]  │
│    │
│ [Execute]   │
│       │
│ Response:   │
│ 200 OK      │
└─────────────┘
```

**Tips for Mobile:**
- Use landscape mode for better view
- Scroll horizontally to see full JSON
- Tap to expand/collapse sections

---

## 💡 Pro Tips | نصائح احترافية

### Tip 1: Use Swagger's "Try it out" Extensively
```
✅ DO: Test every endpoint
✅ DO: Try valid and invalid data
✅ DO: Check all response codes
✅ DO: Copy curl commands for automation
```

### Tip 2: Save Your Tokens
```
Keep a note file with:
- Your test email
- Your test password
- Your current token (with timestamp)
```

### Tip 3: Use Browser DevTools
```
Press F12 to open DevTools
→ Network tab shows actual HTTP requests
→ Console shows any JavaScript errors
→ Helps debug authorization issues
```

### Tip 4: Export Swagger JSON
```
Access: https://localhost:7186/swagger/v1/swagger.json
Use for:
- Code generation tools
- API documentation generators
- Client library creation
```

---

## ✅ Success Criteria | معايير النجاح

You've successfully tested the API when:

1. ✅ Can access Swagger UI
2. ✅ Can register a new user
3. ✅ Can login and receive token
4. ✅ Can authorize with token
5. ✅ Can access protected endpoints
6. ✅ Can see detailed documentation
7. ✅ All status codes make sense
8. ✅ Error messages are clear
9. ✅ Response data matches expectations
10. ✅ Can logout successfully

---

**Happy Testing! 🎉**
**اختبار موفق! 🎉**

For more details, see:
- `SWAGGER_TESTING_GUIDE.md` - Complete guide
- `API_QUICK_REFERENCE.md` - Quick reference
- `README.md` - Documentation overview

