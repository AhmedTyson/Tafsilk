# 🎴 Tailor Registration - Quick Reference Card

## 🔄 The Two Main Flows

### **Condition 1: Complete Registration** ✅
```
Register → Evidence Page → Submit → Pending → Admin Approves → Login → Dashboard
```

### **Condition 2: Incomplete Registration** 🔄
```
Register → Evidence Page → Exit → Login Attempt → Auto-Redirect → Evidence Page → Submit → Same as Condition 1
```

---

## 📍 Pages & URLs

| Page | URL | Purpose |
|------|-----|---------|
| Registration | `/Account/Register` | Initial sign-up |
| Evidence | `/Account/ProvideTailorEvidence` | Submit ID + Portfolio |
| Login | `/Account/Login` | Authentication |
| Dashboard | `/Dashboards/Tailor` | Main hub (after approval) |
| Admin Review | `/AdminDashboard/TailorVerification` | Admin approval |

---

## 🎯 Account States

| State | User.IsActive | TailorProfile | Can Login? | Action Required |
|-------|---------------|---------------|------------|-----------------|
| Registered | `false` | ❌ None | ❌ | Submit Evidence |
| Pending | `false` | ✅ Exists (`IsVerified=false`) | ❌ | Wait for Admin |
| Approved | `true` | ✅ Exists (`IsVerified=true`) | ✅ | Can Use Platform |

---

## 💬 Key Arabic Messages

### **Registration**
```
"تم إنشاء حسابك بنجاح! يجب تقديم الأوراق الثبوتية لإكمال التسجيل"
// Account created! Must provide evidence to complete registration
```

### **Evidence Submitted**
```
"تم تقديم الأوراق الثبوتية بنجاح! سيتم مراجعة طلبك خلال 24-48 ساعة"
// Evidence submitted! Your request will be reviewed within 24-48 hours
```

### **Login (No Evidence)**
```
"يجب تقديم الأوراق الثبوتية لإكمال التسجيل قبل تسجيل الدخول"
// Must provide evidence to complete registration before login
```

### **Login (Pending Approval)**
```
"حسابك قيد المراجعة من قبل الإدارة. سيتم إشعارك عند الموافقة"
// Your account is under admin review. You'll be notified upon approval
```

### **Login Success**
```
"تم تسجيل الدخول بنجاح"
// Login successful
```

---

## 🔐 Required Evidence

| Item | Required? | Max Size | Formats |
|------|-----------|----------|---------|
| ID Document | ✅ Required | 10MB | .pdf, .doc, .docx, .jpg, .png |
| Portfolio Images | ✅ Required (3-10) | 5MB each | .jpg, .jpeg, .png, .gif, .webp |
| Workshop Name | ✅ Required | - | Text |
| Address | ✅ Required | - | Text |
| City | ✅ Required | - | Text |
| Description | ✅ Required | - | Text (max 1000 chars) |

---

## ⚠️ Common Errors

| Error | Arabic Message | Solution |
|-------|----------------|----------|
| No ID | "يجب تحميل صورة الهوية الشخصية" | Upload ID document |
| No Portfolio | "يجب تحميل على الأقل صورة واحدة" | Upload at least 1 image |
| Too Many Images | "يمكن تحميل 10 صور كحد أقصى" | Max 10 images |
| File Too Large | "حجم الملف كبير جداً" | Reduce file size |
| Double Submission | "تم تقديم الأوراق الثبوتية بالفعل" | Already submitted |

---

## 🛡️ Security Features

✅ **Double Submission Prevention**
✅ **File Upload Validation** (size, type, content)
✅ **XSS Protection** (input sanitization)
✅ **SQL Injection Prevention**
✅ **Directory Traversal Prevention**
✅ **Password Strength Validation**

---

## 🧪 Quick Test

### **Test Condition 1**
```bash
1. Go to /Account/Register
2. Select "Tailor"
3. Fill form → Submit
4. Should redirect to /Account/ProvideTailorEvidence
5. Upload ID + 3 images → Submit
6. Should see success message
7. Try login → Should be blocked (pending)
8. Admin approves
9. Login again → Should succeed
10. Should redirect to /Dashboards/Tailor ✅
```

### **Test Condition 2**
```bash
1. Go to /Account/Register
2. Select "Tailor"
3. Fill form → Submit
4. Should redirect to /Account/ProvideTailorEvidence
5. Close browser (don't submit)
6. Go to /Account/Login
7. Enter credentials → Submit
8. Should auto-redirect to /Account/ProvideTailorEvidence
9. Message: "يجب تقديم الأوراق الثبوتية..."
10. Complete evidence form → Submit
11. Follow Condition 1 from step 6 ✅
```

---

## 📊 Database Check

### **After Registration**
```sql
SELECT Id, Email, IsActive, RoleId FROM Users WHERE Email = 'tailor@test.com';
-- IsActive should be FALSE

SELECT * FROM TailorProfiles WHERE UserId = (SELECT Id FROM Users WHERE Email = 'tailor@test.com');
-- Should be EMPTY (not created yet)
```

### **After Evidence Submission**
```sql
SELECT Id, Email, IsActive FROM Users WHERE Email = 'tailor@test.com';
-- IsActive should still be FALSE

SELECT UserId, IsVerified, ShopName FROM TailorProfiles WHERE UserId = (SELECT Id FROM Users WHERE Email = 'tailor@test.com');
-- Should EXIST with IsVerified = FALSE
```

### **After Admin Approval**
```sql
SELECT Id, Email, IsActive FROM Users WHERE Email = 'tailor@test.com';
-- IsActive should be TRUE ✅

SELECT UserId, IsVerified, VerifiedAt FROM TailorProfiles WHERE UserId = (SELECT Id FROM Users WHERE Email = 'tailor@test.com');
-- IsVerified should be TRUE ✅
-- VerifiedAt should have timestamp
```

---

## 🎯 Key Code Locations

### **Registration Logic**
```csharp
// File: AccountController.cs
[HttpPost("Register")]
if (role == RegistrationRole.Tailor) {
    return RedirectToAction("ProvideTailorEvidence");
}
```

### **Login Check (Condition 2)**
```csharp
// File: AccountController.cs
[HttpPost("Login")]
if (err == "TAILOR_INCOMPLETE_PROFILE") {
    return RedirectToAction("ProvideTailorEvidence");
}
```

### **Evidence Submission**
```csharp
// File: AccountController.cs
[HttpPost("ProvideTailorEvidence")]
// Creates TailorProfile
user.IsActive = false; // Keeps inactive
```

### **Admin Approval**
```csharp
// File: AdminDashboardController.cs
[HttpPost("VerifyTailor")]
tailor.Verify(DateTime.UtcNow);
user.IsActive = true; // Activates user
```

---

## 📞 Quick Help

| Issue | Solution |
|-------|----------|
| Can't login after registration | Check if you submitted evidence |
| Stuck on evidence page | Must complete all required fields |
| Evidence already submitted | Wait for admin approval (24-48 hours) |
| Login says "pending review" | Admin hasn't approved yet |
| API registration blocked | Tailors must use web interface |

---

## ✅ Checklist

- [ ] Registration redirects to Evidence Page
- [ ] Evidence form validates all fields
- [ ] Cannot skip evidence submission
- [ ] Cannot login without evidence
- [ ] Cannot login without admin approval
- [ ] Dashboard accessible after approval
- [ ] All messages in Arabic
- [ ] Double submission prevented

---

**Quick Ref Version**: 1.0
**Last Updated**: January 2025
**Build**: ✅ Passing

---

Print this card and keep it handy! 🎴
