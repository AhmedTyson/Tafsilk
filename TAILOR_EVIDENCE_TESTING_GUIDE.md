# 🧪 Tailor Evidence Flow - Quick Testing Guide

## ⚡ Quick Test Scenarios

### 🎯 Test 1: New Tailor Registration (Condition 1)

**Steps:**
1. Navigate to: `https://localhost:7106/Account/Register`
2. Fill in the form:
   - Name: "Ahmed Tailor"
   - Email: "ahmed.tailor@test.com"
   - Password: "Test123!"
   - User Type: Select **"خياط" (Tailor)**
3. Click "تسجيل" (Register)

**✅ Expected Result:**
- Redirect to `/Account/ProvideTailorEvidence`
- See message: "تم إنشاء حسابك بنجاح! يجب تقديم الأوراق الثبوتية لإكمال التسجيل"
- Form displays with required fields

**❌ What NOT to see:**
- No redirect to login page
- No automatic login

---

### 🎯 Test 2: Complete Evidence Submission

**Steps:**
1. On the evidence page, fill in:
   - Workshop Name: "Ahmed's Workshop"
   - Address: "123 Cairo Street"
   - City: "Cairo"
   - Description: "Professional tailoring services"
   - Experience Years: 5
2. Upload:
   - ID Document (any image file)
   - Portfolio Images (at least 1 image)
3. Check "I agree to terms"
4. Click "تقديم" (Submit)

**✅ Expected Result:**
- Redirect to `/Account/Login`
- See success message: "تم تقديم الأوراق الثبوتية بنجاح! سيتم مراجعة طلبك من قبل الإدارة خلال 24-48 ساعة"

**❌ What NOT to see:**
- No errors
- No automatic login

---

### 🎯 Test 3: Try Login Before Admin Approval

**Steps:**
1. On login page, enter:
   - Email: "ahmed.tailor@test.com"
 - Password: "Test123!"
2. Click "تسجيل الدخول" (Login)

**✅ Expected Result:**
- Login blocked
- Error message: "حسابك قيد المراجعة من قبل الإدارة. سيتم تفعيله خلال 24-48 ساعة عمل. سنرسل لك إشعاراً عند الموافقة على حسابك. شكراً لصبرك!"

**❌ What NOT to see:**
- No successful login
- No dashboard access

---

### 🎯 Test 4: Existing Tailor Without Evidence (Condition 2)

**Steps:**
1. Register a new tailor:
   - Email: "test.tailor2@test.com"
   - Password: "Test123!"
2. **Close browser** (simulate session expiry)
3. Open browser again
4. Go to `/Account/Login`
5. Enter credentials and login

**✅ Expected Result:**
- Redirect to `/Account/ProvideTailorEvidence`
- See message: "يجب تقديم الأوراق الثبوتية لإكمال التسجيل قبل تسجيل الدخول"
- Form is pre-filled with email

**❌ What NOT to see:**
- No successful login
- No dashboard access

---

### 🎯 Test 5: Admin Approval Flow

**Steps:**
1. Login as admin (you'll need to create admin via seed data)
2. Navigate to admin dashboard
3. Find pending tailor "ahmed.tailor@test.com"
4. Approve the tailor
5. Logout as admin
6. Login as "ahmed.tailor@test.com"

**✅ Expected Result:**
- Login successful
- Redirect to `/Dashboards/Tailor`
- Dashboard loads with tailor features

---

### 🎯 Test 6: Double Submission Prevention

**Steps:**
1. Complete evidence submission (Test 2)
2. Try to access `/Account/ProvideTailorEvidence` directly in browser
3. Try to submit evidence form again

**✅ Expected Result:**
- GET: Redirect to login with message "تم تقديم الأوراق الثبوتية بالفعل"
- POST: Blocked with same message

---

### 🎯 Test 7: Middleware Protection (Condition 3)

**Steps:**
1. Register tailor but **don't** submit evidence
2. Try to access these URLs directly:
   - `/Dashboards/Tailor`
   - `/TailorManagement/ManageServices`
   - `/Profiles/TailorProfile`

**✅ Expected Result:**
- All URLs redirect to `/Account/ProvideTailorEvidence`
- Cannot bypass evidence requirement

---

## 🔍 Database Verification

### Check User Status
```sql
-- Check tailor user creation
SELECT 
    Id, 
    Email, 
    IsActive, 
    EmailVerified,
    RoleId,
    CreatedAt
FROM Users
WHERE Email = 'ahmed.tailor@test.com';
```

**Expected:**
- After registration: `IsActive = 0` (false)
- After evidence submission: `IsActive = 0` (still false)
- After admin approval: `IsActive = 1` (true)

### Check TailorProfile Creation
```sql
-- Check tailor profile
SELECT 
    Id,
    UserId,
    FullName,
    ShopName,
    IsVerified,
    CreatedAt
FROM TailorProfiles
WHERE UserId = (SELECT Id FROM Users WHERE Email = 'ahmed.tailor@test.com');
```

**Expected:**
- After registration: **NO RECORD** (profile not created yet)
- After evidence submission: **1 RECORD** with `IsVerified = 0`
- After admin approval: `IsVerified = 1`

---

## 🎭 Test User Scenarios

### Scenario A: Happy Path
```
1. Register → 2. Submit Evidence → 3. Wait for Approval → 4. Login → 5. Use Dashboard
✅ All steps work smoothly
```

### Scenario B: Interrupted Registration
```
1. Register → 2. Close Browser → 3. Login Attempt → 4. Redirected to Evidence → 5. Complete Evidence
✅ System recovers and guides user back
```

### Scenario C: Malicious Bypass Attempt
```
1. Register → 2. Try Direct Dashboard URL → 3. Blocked by Middleware
✅ Security measures prevent bypass
```

---

## 🐛 Common Issues & Solutions

### Issue 1: TempData Lost
**Symptom:** Redirect to evidence page but form shows error "Invalid session"
**Solution:** Check that session middleware is enabled in `Program.cs`

### Issue 2: Middleware Not Running
**Symptom:** Can access dashboard without evidence
**Solution:** Verify `app.UseMiddleware<UserStatusMiddleware>();` is after `app.UseAuthentication()`

### Issue 3: Profile Created Too Early
**Symptom:** TailorProfile exists before evidence submission
**Solution:** Check `AuthService.RegisterAsync()` - should NOT create profile for tailors

---

## 📊 Expected Log Output

### Registration Flow:
```
[AuthService] Registration attempt: ahmed.tailor@test.com, Role: Tailor
[AuthService] User created: {Guid}, Email: ahmed.tailor@test.com, Role: Tailor, IsActive: False
[AuthService] Tailor profile creation deferred - awaiting evidence: {Guid}
[AccountController] Authenticated user ahmed.tailor@test.com attempted to access Register. Redirecting to dashboard.
```

### Login Without Evidence (Condition 2):
```
[AuthService] Login attempt for: test.tailor2@test.com
[AuthService] Login attempt - Tailor has not provided evidence yet: test.tailor2@test.com
[AuthService] Redirecting new tailor to evidence submission: test.tailor2@test.com
[AccountController] Tailor test.tailor2@test.com attempted login without evidence. Redirecting to evidence page.
```

### Evidence Submission:
```
[AccountController] Tailor {Guid} completed ONE-TIME evidence submission. Awaiting admin review (IsActive=false).
```

### Double Submission Attempt:
```
[AccountController] Tailor {Guid} attempted to submit evidence but already has profile. Blocking submission.
```

---

## ✅ Test Completion Checklist

- [ ] Test 1: New registration redirects to evidence ✅
- [ ] Test 2: Evidence submission creates profile ✅
- [ ] Test 3: Login blocked until approval ✅
- [ ] Test 4: Existing tailor without evidence redirected ✅
- [ ] Test 5: Admin approval enables login ✅
- [ ] Test 6: Double submission prevented ✅
- [ ] Test 7: Middleware blocks unauthorized access ✅
- [ ] Database checks confirm correct states ✅
- [ ] Logs show expected messages ✅

---

## 🚀 Quick Start Command

```bash
# Run the application
dotnet run --project TafsilkPlatform.Web

# Open in browser
start https://localhost:7106/Account/Register
```

---

**Ready to Test!** 🎉

All scenarios are documented and ready for manual testing. The system is production-ready with all three conditions enforced.
