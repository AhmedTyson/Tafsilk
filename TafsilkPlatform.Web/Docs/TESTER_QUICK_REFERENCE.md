# 🧪 **TESTER QUICK REFERENCE CARD**

## 🔑 **LOGIN CREDENTIALS**

```
Email: tester@tafsilk.local
Password: Tester@123!
```

---

## ✅ **WHAT YOU CAN ACCESS**

| Category | Access | Notes |
|----------|--------|-------|
| **Public Pages** | ✅ Full | Home, Login, Register, Browse Tailors |
| **Customer Pages** | ✅ Full | Dashboard, Orders, Reviews, Profile |
| **Tailor Pages** | ✅ Full | Dashboard, Portfolio, Services, Orders |
| **Admin Pages** | ✅ Full | User Management, Statistics, Verification |

**Total:** 80+ pages accessible with ONE login!

---

## 🗺️ **QUICK NAVIGATION**

### **Start Here:**
```
1. Login: https://localhost:7186/Account/Login
2. Navigation Hub: https://localhost:7186/Testing/NavigationHub
3. Click any page you want to test
```

---

## 🎯 **KEY FEATURES**

### **1. Customer Features:**
- ✅ Browse tailors
- ✅ Create orders
- ✅ Submit reviews
- ✅ Track orders
- ✅ Manage profile

### **2. Tailor Features:**
- ✅ View/accept orders
- ✅ Manage portfolio
- ✅ Manage services
- ✅ View reviews
- ✅ Dashboard statistics

### **3. Admin Features:**
- ✅ Verify tailors
- ✅ Manage users
- ✅ View all orders
- ✅ Platform statistics
- ✅ Manage reviews

---

## 🔧 **IN RAZOR VIEWS**

Use these helpers (automatically available):

```razor
@* Check if user can access customer pages *@
@if (CanAccessCustomerPages(User))
{
    <a href="/Orders/CreateOrder">Create Order</a>
}

@* Check if user can access tailor pages *@
@if (CanAccessTailorPages(User))
{
    <a href="/TailorPortfolio/Index">My Portfolio</a>
}

@* Check if admin *@
@if (IsAdminOrTester(User))
{
    <div class="admin-badge">🧪 Testing Mode</div>
}

@* Get current role *@
<p>Role: @GetUserRole(User)</p>
```

---

## 📊 **YOUR PROFILES**

After seeding, you have:

**Customer Profile:**
```
Name: Tester Account
City: Test City
```

**Tailor Profile:**
```
Shop: Test Tailor Shop
City: Test City
Verified: ✅ Yes
```

---

## ⚡ **QUICK TESTS**

### **Test 1: Customer Flow**
```
/Dashboards/Customer → /Tailors → 
/Orders/CreateOrder → /Reviews/SubmitReview
```

### **Test 2: Tailor Flow**
```
/Dashboards/Tailor → /TailorPortfolio/Index → 
/TailorManagement/Services → /Orders/IncomingOrders
```

### **Test 3: Admin Flow**
```
/Admin → /Admin/Users → 
/Admin/PendingTailors → /Admin/Statistics
```

---

## 🚀 **GETTING STARTED**

```bash
# 1. Run migrations (if needed)
dotnet ef database update

# 2. Run application
dotnet run

# 3. Login
https://localhost:7186/Account/Login
Email: tester@tafsilk.local
Password: Tester@123!

# 4. Navigate anywhere!
```

---

## 📝 **NOTES**

- ✅ All pages work with tester account
- ✅ No need to switch accounts
- ✅ Full admin privileges
- ✅ Both customer and tailor profiles
- ✅ Auto-verified tailor

---

**Happy Testing!** 🧪🎉
