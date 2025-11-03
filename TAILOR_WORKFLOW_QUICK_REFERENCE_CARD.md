# 🎯 Tailor Workflow Quick Reference Card

## 📋 One-Page Cheat Sheet

### 🚀 Registration to Dashboard (Happy Path)
```
Register → Evidence → Login Attempt (Pending) → Admin Approval → Login Success → Dashboard
  2 min      5 min    Blocked      2-3 days          Success!       ✅
```

---

## 🗂️ All Views - Organized by Phase

### Phase 1: Pre-Authentication
| View | Route | Purpose | Required Data |
|------|-------|---------|---------------|
| Register | `/Account/Register` | Initial signup | Name, Email, Pass, Phone |
| ProvideTailorEvidence | `/Account/ProvideTailorEvidence` | Upload documents | ID, 3+ Portfolio, Business Info |
| Login | `/Account/Login` | Authenticate | Email, Password |
| VerifyEmail | `/Account/VerifyEmail?token=...` | Email confirmation | Token from email |

### Phase 2: Post-Authentication (Pending Approval)
| View | Route | Purpose | Access Level |
|------|-------|---------|--------------|
| Tailor Dashboard | `/Dashboards/Tailor` | View stats (limited) | ⚠️ Shows "Pending" banner |
| EditTailorProfile | `/Profiles/EditTailorProfile` | Update profile | ✅ Can edit info |
| ManagePortfolio | `/TailorManagement/ManagePortfolio` | Add more images | ✅ Can add images |

### Phase 3: Post-Approval (Full Access)
| View | Route | Purpose | Features |
|------|-------|---------|----------|
| Tailor Dashboard | `/Dashboards/Tailor` | Main hub | Stats, orders, activity |
| GettingStarted | `/TailorManagement/GettingStarted` | Onboarding guide | 4-step setup |
| ManageServices | `/TailorManagement/ManageServices` | Services CRUD | Add/Edit/Delete services |
| ManagePricing | `/TailorManagement/ManagePricing` | Set prices | Custom pricing |
| ViewPublicProfile | `/TailorPortfolio/ViewPublicTailorProfile/{id}` | Public-facing page | Customer view |

### Admin Views
| View | Route | Purpose |
|------|-------|---------|
| TailorVerification | `/AdminDashboard/TailorVerification` | Pending tailors list |
| ReviewTailor | `/AdminDashboard/ReviewTailor/{id}` | Approve/Reject |

---

## 🔄 Redirect Paths - At a Glance

```
┌────────────────────────────────────────────────────────────┐
│ FROM  → TO           │ TRIGGER   │
├────────────────────────────────────────────────────────────┤
│ Register        → Evidence Page         │ After signup (Tailor)       │
│ Login (no prof) → Evidence Page      │ No TailorProfile            │
│ Login (pending) → Login (Error)         │ IsActive=false        │
│ Login (approved)→ Dashboard   │ All checks pass ✅          │
│ Any /Dashboard  → Evidence Page │ Middleware: No profile      │
│ Evidence Submit → Login Page            │ Profile created ✅    │
│ Email Link      → Login Page   │ Email verified ✅           │
└────────────────────────────────────────────────────────────┘
```

---

## 🔐 Authentication States

### State 1: Registered (No Evidence)
```
User.IsActive = ❌
TailorProfile = ❌
Result: Redirect to Evidence Page
```

### State 2: Evidence Submitted (Pending)
```
User.IsActive = ✅
TailorProfile = ✅
TailorProfile.IsVerified = ❌
Result: Login blocked - "حسابك قيد المراجعة..."
```

### State 3: Approved (Full Access)
```
User.IsActive = ✅
TailorProfile = ✅
TailorProfile.IsVerified = ✅
Result: Full dashboard access
```

---

## 📊 Database Tables - Key Fields

### Users Table
```
Id (PK)
Email
PasswordHash
RoleId (FK) → Roles
IsActive      ← Admin approval
IsDeleted
EmailVerified ← Separate from login
CreatedAt
LastLoginAt
```

### TailorProfiles Table
```
Id (PK)
UserId (FK) → Users
FullName
ShopName
Address, City
IsVerified ← Admin approval
VerifiedAt
ProfilePictureData
CreatedAt
```

### PortfolioImages Table
```
PortfolioImageId (PK)
TailorId (FK) → TailorProfiles
ImageUrl
IsBeforeAfter
UploadedAt
```

### TailorServices Table
```
TailorServiceId (PK)
TailorId (FK) → TailorProfiles
ServiceName
BasePrice
EstimatedDuration
IsActive
```

---

## 🛡️ Middleware Protection

### UserStatusMiddleware.cs
**Runs on:** Every request to `/Dashboards/*` and `/TailorManagement/*`

**Checks:**
1. ✅ Is authenticated?
2. ✅ Is Tailor role?
3. ✅ Has TailorProfile?
4. ✅ Is Active?

**Actions:**
- No profile → Redirect to Evidence
- Inactive → Set ViewData flag (allow access, show banner)

---

## 🎯 Key Controller Methods

### AccountController
```csharp
Register(POST)→ Creates User, redirects to Evidence
ProvideTailorEvidence(POST) → Creates TailorProfile, sets IsActive=true
Login(POST)              → Validates & signs in OR redirects to Evidence
VerifyEmail(GET)     → Marks email verified
Logout(POST)        → Signs out
```

### DashboardsController
```csharp
Tailor()                 → Shows dashboard (checks middleware first)
```

### AdminDashboardController
```csharp
TailorVerification()     → Lists pending tailors
ReviewTailor(id)  → Shows evidence for review
ApproveTailor(POST)      → Sets IsVerified=true
```

### TailorManagementController
```csharp
GettingStarted()      → Onboarding guide
ManagePortfolio()        → Portfolio list
AddPortfolioImage()      → Upload new image
ManageServices()         → Services list
AddService()→ Create new service
ManagePricing()          → Set custom pricing
```

### ProfilesController
```csharp
EditTailorProfile()      → Update profile form
GetTailorProfilePicture(id) → Serves profile image
```

---

## 📝 Required Documents for Evidence

### Mandatory ✅
1. **ID Document** (1 file)
   - National ID or Passport
   - Clear image
   - Accepted: JPG, PNG

2. **Portfolio Images** (3+ files)
   - Previous work samples
   - High quality
   - Shows skill level

3. **Business Information**
   - Workshop name
   - Address
   - City
   - Description
   - Phone number

### Optional ⚠️
- Commercial registration
- Experience certificates
- Additional documents
- Years of experience

---

## 🔔 Notification Flow

### Email Notifications
1. **Registration** → Verification email (24h expiry)
2. **Evidence Submitted** → Confirmation email
3. **Admin Approved** → Approval email ("تهانينا!")
4. **Admin Rejected** → Rejection email with reason

### In-App Notifications
- Dashboard banner: "حسابك قيد المراجعة..."
- Success messages: "تم تحديث ملفك الشخصي بنجاح!"
- Error messages: "يجب إكمال عملية التحقق..."

---

## 🚨 Common Errors & Solutions

### Error: "يجب إكمال عملية التحقق..."
**Cause:** Tailor has no TailorProfile
**Solution:** Complete evidence submission at `/Account/ProvideTailorEvidence`

### Error: "حسابك قيد المراجعة من قبل الإدارة..."
**Cause:** IsActive=false (pending admin approval)
**Solution:** Wait 2-3 business days for admin review

### Error: "جلسة غير صالحة. يرجى تسجيل الدخول..."
**Cause:** TempData expired or user not authenticated
**Solution:** Login first or re-register

### Error: "تم تقديم الأوراق الثبوتية بالفعل"
**Cause:** Duplicate submission attempt
**Solution:** Login and access dashboard

---

## ⏱️ Timeline Estimates

| Stage | Duration | User Action Required |
|-------|----------|---------------------|
| Registration | 2-3 minutes | Enter personal info |
| Evidence Upload | 5-10 minutes | Upload documents, fill business details |
| Email Verification | 1 minute | Click link in email |
| Admin Review | 2-3 business days | ⏳ Wait (no action) |
| Profile Setup | 15-30 minutes | Add services, portfolio, complete profile |
| First Order | Varies | Wait for customer orders |

---

## 📱 Pages by User Type

### Customers Can Access
- `/Home/*` - Public pages
- `/Account/Login` - Login
- `/Account/Register` - Signup
- `/TailorPortfolio/ViewPublicTailorProfile/{id}` - Browse tailors
- `/Dashboards/Customer` - Customer dashboard

### Tailors Can Access (Pre-Approval)
- `/Account/ProvideTailorEvidence` - Evidence submission
- `/Dashboards/Tailor` - Dashboard (with pending banner)
- `/Profiles/EditTailorProfile` - Profile editing
- `/TailorManagement/ManagePortfolio` - Add more images

### Tailors Can Access (Post-Approval)
- **ALL** Tailor features unlocked
- Receive customer orders
- Full dashboard statistics
- Public profile visible to customers

### Admins Can Access
- `/AdminDashboard/*` - All admin pages
- Review and approve/reject tailors
- Manage users, disputes, settings

---

## 🔗 Navigation Sidebar (Tailor Dashboard)

```
┌─────────────────────────┐
│ 🏠 Dashboard            │ → /Dashboards/Tailor
│ 🖼️ Portfolio            │ → /TailorManagement/ManagePortfolio
│ 🛎️ Services     │ → /TailorManagement/ManageServices
│ ⚙️ Settings        │ → /Profiles/EditTailorProfile
│ 🚪 Logout   │ → /Account/Logout
└─────────────────────────┘
```

---

## 🎨 UI Indicators

### Verification Badge
```
✅ Verified:   Green badge "حساب موثق"
⏳ Pending:    Yellow badge "في انتظار التوثيق"
```

### Account Status
```
✅ Active:     Can login, full access
❌ Inactive:   Cannot login (except evidence page)
🗑️ Deleted:    Account removed (cannot login)
```

### Email Status
```
✅ Verified:   Email confirmed
❌ Unverified: Pending email confirmation
```

---

## 📞 Support Information

**For Tailors:**
- Email: support@tafsilk.com
- Phone: +20 123 456 7890
- Hours: Saturday - Thursday, 9 AM - 9 PM

**Common Questions:**
1. How long does approval take? → 2-3 business days
2. Can I edit profile before approval? → Yes
3. Can I add more portfolio images later? → Yes
4. What if my evidence is rejected? → Admin will email reason

---

## 🎯 Success Metrics (Post-Approval)

Dashboard shows:
- **Active Orders:** Current projects
- **Completed Orders:** Finished projects
- **New Orders:** Pending requests
- **Monthly Revenue:** Earnings this month
- **Average Rating:** Customer reviews
- **Profile Views:** Visibility metric

---

## 🔒 Security Features

1. **Password Hashing:** PasswordHasher.Hash() (bcrypt-based)
2. **Email Verification:** Token-based (24h expiry)
3. **Anti-CSRF:** `@Html.AntiForgeryToken()` on all forms
4. **Authorization:** `[Authorize(Policy = "TailorPolicy")]`
5. **Input Validation:** Server-side + client-side
6. **Document Storage:** Secure blob storage in database

---

## 📚 Related Documentation Files

1. `COMPLETE_TAILOR_WORKFLOW_AND_NAVIGATION_MAP.md` - Detailed workflow
2. `TAILOR_DECISION_TREE_VISUAL_FLOWCHART.md` - Visual flowchart
3. `TAILOR_AUTHENTICATION_FLOW_ANALYSIS.md` - Auth analysis
4. `TAILOR_EVIDENCE_REDIRECT_FIX.md` - Recent fixes
5. `TAILOR_VERIFICATION_COMPLETE_FLOW.md` - Verification process

---

**Quick Tip:** Bookmark this page for instant reference during development!

**Last Updated:** Current codebase snapshot
**Maintained By:** Development team
