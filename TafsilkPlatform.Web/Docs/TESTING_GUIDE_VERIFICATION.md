# Quick Testing Guide - Tailor Verification System

## 🚀 Quick Start Testing

### Test Scenario 1: Complete Tailor Registration (Happy Path)

#### Step 1: Register New Tailor
```
URL: https://localhost:PORT/Account/Register
User Type: Tailor
Name: محمد أحمد
Email: mohammed@example.com
Password: Test@1234
Phone: +966501234567
```

#### Step 2: Complete Profile (Step 1 - Identity)
```
National ID: 1234567890
Full Legal Name: محمد أحمد علي
Nationality: سعودي
Date of Birth: 1990-01-01
Workshop Name: ورشة محمد للخياطة
Workshop Type: tailoring
Phone: +966501234567
City: Riyadh
Address: حي الملز، شارع الأمير محمد
Description: ورشة خياطة متخصصة في الثياب الرجالية
Experience Years: 5
```

#### Step 3: Upload Documents (Step 2)
```
Required:
  - ID Front: Upload clear image of national ID (front)
  - Portfolio: Upload 3-5 high-quality work samples

Optional:
  - ID Back: Upload back of national ID
  - Commercial Registration: Upload CR document
  - Professional License: Upload license if available
```

#### Step 4: Review & Submit (Step 3)
```
- Review all information
- Check uploaded documents count
- Agree to terms and conditions
- Click "تسجيل الورشة" (Register Workshop)
```

**Expected Result**: 
- Auto-login successful
- Redirected to Tailor Dashboard
- See "Pending Approval" notice
- Can browse but limited functionality

---

### Test Scenario 2: Admin Review & Approval

#### Step 1: Login as Admin
```
URL: https://localhost:PORT/Account/Login
Email: admin@tafsilk.com
Password: Admin@123
```

#### Step 2: Navigate to Verification Queue
```
URL: https://localhost:PORT/AdminDashboard/TailorVerification
Expected: List of pending tailors
```

#### Step 3: Review Tailor Documents
```
1. Click "Review" on a pending tailor
2. Check identity information:
   ✓ National ID number matches format
   ✓ Legal name is clear
   ✓ Workshop details are complete

3. View each document:
   - Click "View Document" for ID Front → Opens in new tab
   - Verify image quality and readability
   - Check portfolio images (min 3)
   - Review any optional documents

4. Make verification checklist:
   ✓ Has services listed
   ✓ Has portfolio images
   ✓ Has bio/description
   ✓ Experience specified
   ✓ Address provided
```

#### Step 4: Approve or Reject

**For Approval:**
```
1. Click "Approve Tailor" button
2. Optionally add notes (e.g., "Documents verified, profile looks professional")
3. Confirm approval
```

**Expected Result**:
- Success message displayed
- Tailor removed from pending queue
- IsVerified = true in database
- VerifiedAt timestamp set

**For Rejection:**
```
1. Click "Reject Application" button
2. Enter rejection reason (required):
   Example: "ID document is not clear, please provide a better quality image"
3. Confirm rejection
```

**Expected Result**:
- Success message displayed
- Tailor status = Rejected
- Rejection reason saved

---

### Test Scenario 3: Post-Approval Access

#### Step 1: Login as Approved Tailor
```
URL: https://localhost:PORT/Account/Login
Use the tailor credentials from Test Scenario 1
```

#### Step 2: Verify Full Access
```
✓ Dashboard loads without "Pending Approval" notice
✓ Can edit profile fully
✓ Can add/edit services
✓ Can manage portfolio
✓ Profile appears in public search
✓ Can receive orders
```

#### Step 3: Check Public Profile
```
URL: https://localhost:PORT/Profiles/ViewPublicTailorProfile?id={TailorId}
Expected: Profile is publicly accessible
```

---

## 📊 Database Verification Queries

### Check Verification Record
```sql
SELECT 
    tv.Id,
    tp.ShopName,
    tv.NationalIdNumber,
    tv.FullLegalName,
    tv.Status,
    tv.SubmittedAt,
    tv.ReviewedAt,
    tp.IsVerified
FROM TailorVerifications tv
INNER JOIN TailorProfiles tp ON tv.TailorProfileId = tp.Id
WHERE tp.ShopName = 'ورشة محمد للخياطة';
```

### Check Document Storage
```sql
SELECT 
    tp.ShopName,
 CASE WHEN tv.IdDocumentFrontData IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasIDFront,
    CASE WHEN tv.IdDocumentBackData IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasIDBack,
    CASE WHEN tv.CommercialRegistrationData IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasCR,
    CASE WHEN tv.ProfessionalLicenseData IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasLicense,
    (SELECT COUNT(*) FROM PortfolioImages WHERE TailorId = tp.Id AND IsDeleted = 0) AS PortfolioCount
FROM TailorVerifications tv
INNER JOIN TailorProfiles tp ON tv.TailorProfileId = tp.Id;
```

---

## 🧪 Edge Cases to Test

### Case 1: Duplicate Submission Prevention
```
1. Complete profile as tailor
2. Logout
3. Try to access /Account/CompleteTailorProfile again
Expected: Redirected to login with message "Profile already completed"
```

### Case 2: Invalid File Upload
```
1. Try uploading PDF as ID document (should accept images only)
Expected: Validation error
2. Try uploading file > 5MB
Expected: File size error
```

### Case 3: Incomplete Portfolio
```
1. Upload only 2 portfolio images (less than required 3)
Expected: Cannot proceed to Step 3, validation error shown
```

### Case 4: Missing Required Documents
```
1. Try to proceed to Step 3 without uploading ID front
Expected: Validation error, cannot proceed
```

### Case 5: Admin Document Viewing
```
1. Login as regular user (customer)
2. Try to access: /AdminDashboard/ViewVerificationDocument?tailorId=XXX&documentType=idfront
Expected: Unauthorized/Forbidden
```

---

## ✅ Test Checklist

### Registration & Onboarding
- [ ] Can register as tailor with valid credentials
- [ ] Can enter identity information correctly
- [ ] Can upload ID front document (required)
- [ ] Can upload ID back document (optional)
- [ ] Can upload 3+ portfolio images
- [ ] Can upload commercial registration (optional)
- [ ] Can upload professional license (optional)
- [ ] Form validation works correctly
- [ ] Multi-step navigation works (Previous/Next buttons)
- [ ] Summary page shows all entered data
- [ ] Can submit successfully
- [ ] Auto-login works after submission
- [ ] Redirected to tailor dashboard
- [ ] "Pending Approval" notice displayed

### Admin Review
- [ ] Admin can see pending verification queue
- [ ] Can view tailor details
- [ ] Can view identity information
- [ ] Can view ID front document
- [ ] Can view ID back document (if uploaded)
- [ ] Can view commercial registration (if uploaded)
- [ ] Can view professional license (if uploaded)
- [ ] Can view portfolio images
- [ ] Can approve tailor with notes
- [ ] Can reject tailor with reason
- [ ] Success messages display correctly
- [ ] Tailor removed from queue after action

### Post-Approval
- [ ] Tailor can login after approval
- [ ] "Pending Approval" notice removed
- [ ] Full dashboard access
- [ ] Profile is public
- [ ] Can receive orders
- [ ] Services visible to customers

### Security
- [ ] Non-admin cannot access verification documents
- [ ] Tailor cannot re-submit profile
- [ ] Documents stored securely
- [ ] File upload validation works
- [ ] XSS protection in place
- [ ] SQL injection protection

---

## 🐛 Known Issues & Workarounds

### Issue 1: Upload Progress Not Shown
**Status**: Minor UX issue  
**Workaround**: Wait for upload to complete, no progress bar

### Issue 2: Large File Upload Timeout
**Status**: Configuration needed  
**Fix**: Adjust `MaxRequestBodySize` in Program.cs if needed

---

## 📞 Quick Support Commands

### Reset Tailor Verification Status
```sql
-- Reset for retesting
UPDATE TailorProfiles 
SET IsVerified = 0, VerifiedAt = NULL, UpdatedAt = GETUTCDATE()
WHERE Id = 'TAILOR_PROFILE_ID';

UPDATE TailorVerifications
SET Status = 0, ReviewedAt = NULL, ReviewedByAdminId = NULL, ReviewNotes = NULL
WHERE TailorProfileId = 'TAILOR_PROFILE_ID';
```

### Delete Test Tailor Completely
```sql
-- Use with caution!
DELETE FROM TailorVerifications WHERE TailorProfileId = 'TAILOR_PROFILE_ID';
DELETE FROM PortfolioImages WHERE TailorId = 'TAILOR_PROFILE_ID';
DELETE FROM TailorServices WHERE TailorId = 'TAILOR_PROFILE_ID';
DELETE FROM TailorProfiles WHERE Id = 'TAILOR_PROFILE_ID';
DELETE FROM Users WHERE Id = 'USER_ID';
```

---

## 🎯 Success Criteria

✅ **Registration**: Tailor can complete full registration with identity documents  
✅ **Admin Review**: Admin can view all documents and make approval decisions  
✅ **Approval**: Approved tailors get full platform access  
✅ **Rejection**: Rejected tailors are notified with reason  
✅ **Security**: Only admins can view verification documents  
✅ **UX**: Multi-step form is intuitive and user-friendly  

---

*Last Updated: November 6, 2024*  
*Test Version: 1.0*
