# 🚀 SHOPPING CART - QUICK COMMANDS

## ⚡ **START APPLICATION**

```bash
cd TafsilkPlatform.Web
dotnet run
```

**Access Store:** https://localhost:7186/Store

---

## 📍 **ALL ROUTES**

### **Public (No Login Required)**
```
GET  /Store         # Browse products
GET  /Store/Product/{id}     # Product details
```

### **Customer Only**
```
GET  /Store/Cart       # View cart
GET  /Store/Checkout           # Checkout page
POST /Store/AddToCart           # Add to cart
POST /Store/UpdateCartItem         # Update quantity
POST /Store/RemoveFromCart         # Remove item
POST /Store/ProcessCheckout      # Complete purchase
GET  /Store/api/cart/count  # Get cart count (AJAX)
```

---

## 🧪 **QUICK TEST**

### **1. Seed Products**
```bash
dotnet run
# Products auto-seeded on startup
```

### **2. Register Customer**
```
1. Go to /Account/Register
2. Choose "عميل (Customer)"
3. Complete profile
4. Login
```

### **3. Test Shopping**
```
1. Go to /Store
2. Click any product
3. Click "أضف إلى السلة"
4. Go to /Store/Cart
5. Click "متابعة للدفع"
6. Fill form
7. Click "تأكيد الطلب"
8. Done! ✓
```

---

## 🔧 **DATABASE COMMANDS**

### **Check Products**
```sql
SELECT COUNT(*) FROM Products;
-- Should return 12

SELECT Name, Price, StockQuantity FROM Products;
```

### **Check Cart**
```sql
SELECT * FROM ShoppingCarts WHERE CustomerId = {your-id};
SELECT * FROM CartItems WHERE CartId = {cart-id};
```

### **Check Orders**
```sql
SELECT * FROM Orders WHERE OrderType = 'StoreOrder';
SELECT * FROM OrderItems WHERE ProductId IS NOT NULL;
```

---

## 🐛 **TROUBLESHOOTING**

### **Products Not Showing?**
```sql
-- Check products exist
SELECT COUNT(*) FROM Products;

-- Re-seed if needed
DELETE FROM Products;
-- Restart application (auto-seeds)
```

### **Cart Badge Not Updating?**
```
1. Check browser console for errors
2. Verify endpoint: /Store/api/cart/count
3. Check network tab for AJAX calls
4. Ensure logged in as Customer
```

### **Cannot Add to Cart?**
```
1. Verify logged in as Customer
2. Check product stock > 0
3. Check browser console
4. Verify anti-forgery token
```

---

## 📊 **MONITORING**

### **Check Logs**
```bash
# In terminal where app is running
# Look for:
info: Successfully added product {ProductId} to cart
info: Order {OrderId} created from cart checkout
```

### **Verify Database State**
```sql
-- Active carts
SELECT COUNT(*) FROM ShoppingCarts WHERE IsActive = 1;

-- Cart items
SELECT COUNT(*) FROM CartItems;

-- Store orders
SELECT COUNT(*) FROM Orders WHERE OrderType = 'StoreOrder';

-- Stock levels
SELECT Name, StockQuantity FROM Products WHERE StockQuantity < 10;
```

---

## ⚙️ **CONFIGURATION**

### **Change Free Shipping Threshold**
```csharp
// In StoreService.cs
private decimal CalculateShippingCost(decimal subtotal)
{
    return subtotal >= 500 ? 0 : 25; // Change 500 to your value
}
```

### **Change Tax Rate**
```csharp
// In StoreService.cs
private decimal CalculateTax(decimal subtotal)
{
    return subtotal * 0.15m; // Change 0.15 to your rate
}
```

### **Change Shipping Cost**
```csharp
// In StoreService.cs
return subtotal >= 500 ? 0 : 25; // Change 25 to your cost
```

---

## 🎯 **COMMON TASKS**

### **Add New Product (Manual)**
```sql
INSERT INTO Products (
    ProductId, Name, Description, Price, Category,
    StockQuantity, IsAvailable, CreatedAt, TailorId
) VALUES (
    NEWID(), 'Product Name', 'Description', 299.00, 'Category',
    50, 1, GETDATE(), {system-tailor-id}
);
```

### **Clear All Carts**
```sql
DELETE FROM CartItems;
DELETE FROM ShoppingCarts;
```

### **Reset Stock**
```sql
UPDATE Products SET StockQuantity = 50 WHERE StockQuantity < 10;
```

### **View Sales**
```sql
SELECT 
    p.Name,
    p.SalesCount,
    p.StockQuantity,
    (p.Price * p.SalesCount) AS TotalRevenue
FROM Products p
WHERE p.SalesCount > 0
ORDER BY p.SalesCount DESC;
```

---

## 📱 **TESTING URLS**

```
Homepage:   https://localhost:7186/
Store:           https://localhost:7186/Store
Cart:        https://localhost:7186/Store/Cart
Checkout:        https://localhost:7186/Store/Checkout
My Orders:       https://localhost:7186/orders/my-orders
Register:        https://localhost:7186/Account/Register
Login:           https://localhost:7186/Account/Login
```

---

## 🔑 **IMPORTANT FILES**

```
Controllers:     StoreController.cs
Services:    StoreService.cs
Models:          Product.cs, ShoppingCart.cs, CartItem.cs
Views: Views/Store/*.cshtml
Navigation:      Views/Shared/_UnifiedNav.cshtml
Database:        Migrations/*_AddEcommerceFeatures.cs
```

---

## 📚 **DOCUMENTATION**

```
Complete Guide:     SHOPPING_CART_COMPLETE_PROCESS.md
Test Checklist:     SHOPPING_CART_TEST_CHECKLIST.md
Success Summary:    SHOPPING_CART_SUCCESS_FINAL.md
Store Views:        STORE_VIEWS_COMPLETE.md
Quick Start:     STORE_QUICK_START.md
Visual Flow:        STORE_VISUAL_FLOW.md
```

---

## ✅ **VERIFICATION**

### **Quick Health Check**
```bash
# 1. Build
dotnet build
# Expected: Build succeeded. 0 Error(s)

# 2. Run
dotnet run
# Expected: Application started

# 3. Open Browser
# https://localhost:7186/Store
# Expected: Products display

# 4. Check Database
# SELECT COUNT(*) FROM Products;
# Expected: 12
```

---

## 🎉 **READY TO GO!**

Your shopping cart is **100% complete** and **ready for use**!

**Quick Start:**
1. `dotnet run`
2. Open https://localhost:7186/Store
3. Register as customer
4. Start shopping! 🛍️

**Status:** ✅ **ALL SYSTEMS GO!** 🚀

