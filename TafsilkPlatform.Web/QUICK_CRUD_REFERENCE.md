# 🚀 Quick CRUD Operations Reference - Tafsilk Platform

## Customer Operations

### My Profile
- ➕ Create: `/profile/complete-customer` (POST)
- 👁️ View: `/profile/customer` (GET)

### My Addresses
- ➕ Create: `/profile/addresses/add` (POST)
- 👁️ View All: `/profile/addresses` (GET)
- ✏️ Edit: `/profile/addresses/edit/{id}` (POST)
- 🗑️ Delete: `/profile/addresses/delete/{id}` (POST)
- ⭐ Set Default: `/profile/addresses/set-default/{id}` (POST)

### My Orders
- ➕ Create: `/orders/create` (POST)
- 👁️ View All: `/orders/my-orders` (GET)
- 👁️ View Details: `/orders/{id}` (GET)
- ❌ Cancel: `/orders/{id}/cancel` (POST)

### Shopping Cart
- ➕ Add Item: `/Store/AddToCart` (POST)
- 👁️ View Cart: `/Store/Cart` (GET)
- ✏️ Update Item: `/Store/UpdateCartItem` (POST)
- 🗑️ Remove Item: `/Store/RemoveFromCart` (POST)
- 🗑️ Clear All: `/Store/ClearCart` (POST)

### Shopping
- 👁️ Browse Products: `/Store` (GET)
- 👁️ Product Details: `/Store/Product/{id}` (GET)
- 💳 Checkout: `/Store/Checkout` (GET)
- 💳 Process Payment: `/Store/ProcessCheckout` (POST)

---

## Tailor Operations

### My Profile
- 👁️ View: `/profile/tailor` (GET)
- ✏️ Edit: `/profile/tailor/edit` (POST)

### My Services
- ➕ Add Service: `/tailor/manage/services/add` (POST)
- 👁️ View All: `/tailor/manage/services` (GET)
- ✏️ Edit Service: `/tailor/manage/services/edit/{id}` (POST)
- 🗑️ Delete Service: `/tailor/manage/services/delete/{id}` (POST)
- 💰 Update Pricing: `/tailor/manage/pricing` (POST)

### My Portfolio
- ➕ Add Image: `/tailor/manage/portfolio/add` (POST)
- 👁️ View All: `/tailor/manage/portfolio` (GET)
- ✏️ Edit Image: `/tailor/manage/portfolio/edit/{id}` (POST)
- 🗑️ Delete Image: `/tailor/manage/portfolio/delete/{id}` (POST)
- ⭐ Toggle Featured: `/tailor/manage/portfolio/toggle-featured/{id}` (POST)

### My Orders
- 👁️ View All: `/orders/tailor/manage` (GET)
- 👁️ View Details: `/orders/{id}` (GET)
- ✏️ Update Status: `/orders/{id}/update-status` (POST)

---

## Admin Operations

### User Management
- 👁️ View All Users: `/AdminDashboard/Users` (GET)
- 👁️ User Details: `/AdminDashboard/UserDetails/{id}` (GET)
- 🚫 Suspend User: `/AdminDashboard/SuspendUser/{id}` (POST)
- ✅ Activate User: `/AdminDashboard/ActivateUser/{id}` (POST)
- 🗑️ Delete User: `/AdminDashboard/DeleteUser/{id}` (POST) - Soft delete
- 🔄 Change Role: `/AdminDashboard/UpdateUserRole/{id}` (POST)

### Order Management
- 👁️ View All Orders: `/AdminDashboard/Orders` (GET)
- ❌ Cancel Order: `/AdminDashboard/CancelOrder/{id}` (POST)

### Product Management
- 👁️ View All Products: `/AdminDashboard/Products` (GET)
- 🔄 Toggle Availability: `/AdminDashboard/ToggleProductAvailability/{id}` (POST)
- 🗑️ Delete Product: `/AdminDashboard/DeleteProduct/{id}` (POST)
- 📦 Update Stock: `/AdminDashboard/UpdateProductStock/{id}` (POST)

### Portfolio Review
- 👁️ View All Images: `/AdminDashboard/PortfolioReview` (GET)
- 🗑️ Delete Image: `/AdminDashboard/DeletePortfolioImage/{id}` (POST)

---

## 📋 HTTP Methods Guide

| Symbol | Method | Purpose |
|--------|--------|---------|
| ➕ | POST | Create new item |
| 👁️ | GET | Read/View items |
| ✏️ | POST | Update existing item |
| 🗑️ | POST | Delete item |
| 🔄 | POST | Toggle/Change status |
| ⭐ | POST | Mark as special/featured |
| ❌ | POST | Cancel/Reject |
| ✅ | POST | Approve/Activate |
| 🚫 | POST | Suspend/Block |
| 💰 | POST | Update pricing |
| 💳 | POST | Process payment |
| 📦 | POST | Update inventory |

---

## 🔒 Authorization Requirements

| Role | Access Level |
|------|--------------|
| **Customer** | Profile, Addresses, Orders (own), Shopping Cart, Store |
| **Tailor** | Profile, Services, Portfolio, Orders (assigned to them) |
| **Admin** | Everything + User Management + Content Moderation |

---

## 🛡️ Security Features

- ✅ All POST requests require anti-forgery tokens
- ✅ Role-based authorization on all sensitive endpoints
- ✅ Ownership validation (users can only modify their own data)
- ✅ Admin protection (cannot delete/suspend other admins)
- ✅ Soft delete for most entities (data recovery possible)
- ✅ Input validation on all forms
- ✅ File upload validation (size, type)

---

## 📊 Quick Stats

- **Total Endpoints:** 60+
- **Controllers with CRUD:** 5
- **Entities with Full CRUD:** 7
- **Build Status:** ✅ Successful

---

## 💡 Tips

1. **All deletions are soft deletes** - Data is marked as deleted but not removed
2. **Default addresses** - System automatically manages single default per user
3. **Order status** - Follows strict state machine (see transitions in CRUD_OPERATIONS_SUMMARY.md)
4. **Cart management** - Automatic customer cart creation on first add
5. **Stock management** - Products auto-disable when stock reaches 0

---

**For detailed information, see:** `CRUD_OPERATIONS_SUMMARY.md`
