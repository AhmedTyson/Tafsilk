# 🎯 Tailor Product Management - Quick Reference

**Status:** ✅ READY TO USE

---

## 🚀 QUICK START

### Access Product Management:
```
URL: /tailor/manage/products
Required: Tailor role
```

### URLs Available:
```
/tailor/manage/products              → List all products
/tailor/manage/products/add          → Add new product
/tailor/manage/products/edit/{id}    → Edit product
/tailor/manage/products/delete/{id}  → Delete product (POST)
/tailor/manage/products/image/{id}   → Get product image
```

---

## 📋 FILES CREATED

### ViewModels:
```
✅ ViewModels/TailorManagement/ManageProductsViewModel.cs
   - ManageProductsViewModel
   - ProductItemDto
   - AddProductViewModel
   - EditProductViewModel
   - QuickStockUpdateViewModel
```

### Views:
```
✅ Views/TailorManagement/ManageProducts.cshtml
✅ Views/TailorManagement/AddProduct.cshtml
✅ Views/TailorManagement/EditProduct.cshtml
```

### Controller:
```
✅ Controllers/TailorManagementController.cs
   - Added Product Management section
   - 9 new actions
   - 5 helper methods
```

---

## 🎨 UI COMPONENTS

### ManageProducts.cshtml:
```html
<!-- Statistics Cards -->
- Total Products
- Active Products
- Out of Stock
- Inventory Value

<!-- DataTable -->
- Sortable columns
- Search functionality
- Inline stock update
- Toggle availability
- Edit/Preview/Delete actions

<!-- Modals -->
- Delete confirmation modal
```

### AddProduct.cshtml:
```html
<!-- Sections -->
1. Basic Information (Name, Category, Description)
2. Pricing & Stock (Price, Discount, Quantity)
3. Product Details (Size, Color, Material, Brand)
4. Images (Primary + 5 Additional)
5. SEO (Meta Title, Description)

<!-- Features -->
- Image preview on select
- Auto-fill SEO fields
- Price validation
- Form validation
- Loading states
```

### EditProduct.cshtml:
```html
<!-- Same as AddProduct + -->
- Current image display
- Optional new image upload
- Pre-filled form values
- Update button instead of Create
```

---

## 💻 CODE EXAMPLES

### Add Product:
```csharp
var model = new AddProductViewModel
{
    TailorId = tailorId,
    Name = "ثوب رجالي أبيض",
    Description = "ثوب رجالي فاخر من القطن 100%",
    Price = 299.00m,
    DiscountedPrice = 249.00m,
    Category = "ثوب رجالي",
    SubCategory = "رجالي",
    Size = "L",
    Color = "أبيض",
    Material = "قطن 100%",
    StockQuantity = 10,
    IsAvailable = true,
    IsFeatured = false,
    PrimaryImage = imageFile,
    AdditionalImages = new List<IFormFile> { img1, img2 }
};
```

### Update Stock (AJAX):
```javascript
$.ajax({
    url: '/tailor/manage/products/update-stock/' + productId,
    method: 'POST',
    data: {
        newStock: 25,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        if (response.success) {
            toastr.success('تم تحديث الكمية');
            // Update UI
        }
    }
});
```

### Toggle Availability (AJAX):
```javascript
// Form auto-submits on toggle change
<form asp-action="ToggleProductAvailability" asp-route-id="@productId">
    <input type="checkbox" class="availability-toggle" 
           @(isAvailable ? "checked" : "")
           onchange="this.form.submit()">
</form>
```

---

## 🔧 CATEGORIES & OPTIONS

### Categories:
```
ثوب رجالي, فستان نسائي, بدلة رسمية, عباءة, جلابية
قميص, تنورة, بنطلون, معطف, فستان سهرة
ملابس أطفال, اكسسوارات, أخرى
```

### Sizes:
```
XS, S, M, L, XL, XXL, XXXL, مقاس حر
```

### Materials:
```
قطن 100%, بوليستر, حرير, صوف, كتان
مخلوط, جينز, شيفون, ساتان, قطيفة
```

---

## 📊 DATABASE SCHEMA

### Product Model (Existing):
```csharp
ProductId          Guid
Name               string (200)
Description        string (2000)
Price              decimal
DiscountedPrice    decimal?
Category           string (100)
SubCategory        string (50)
Size               string (50)
Color              string (50)
Material           string (100)
Brand              string (100)
StockQuantity      int
IsAvailable        bool
IsFeatured         bool
ViewCount          int
SalesCount         int
AverageRating      double
ReviewCount        int
PrimaryImageData   byte[]
PrimaryImageContentType  string (100)
AdditionalImagesJson     string (4000)
MetaTitle          string (200)
MetaDescription    string (500)
Slug               string (200)
TailorId           Guid?
CreatedAt          DateTimeOffset
UpdatedAt          DateTimeOffset?
IsDeleted          bool
```

---

## 🔒 SECURITY

### Authorization:
```csharp
[Authorize(Roles = "Tailor")]
```

### Ownership Check:
```csharp
if (tailor.Id != product.TailorId)
    return Unauthorized();
```

### Image Validation:
```csharp
// Types: JPG, PNG, GIF, WEBP
// Max Size: 5 MB
// Validation: IFileUploadService
```

### Delete Protection:
```csharp
// Cannot delete if:
- Product in active orders (not Cancelled/Delivered)
```

---

## 📱 RESPONSIVE DESIGN

### Mobile View:
```
- Stack cards vertically
- Full-width buttons
- Collapsible table columns
- Touch-friendly inputs
- Optimized DataTables
```

---

## 🎯 FEATURES

### Implemented:
- ✅ Create products
- ✅ Upload images (1 primary + 5 additional)
- ✅ Edit all details
- ✅ Update stock inline
- ✅ Toggle availability
- ✅ Soft delete
- ✅ Delete protection
- ✅ Image serving
- ✅ SEO slug generation
- ✅ Statistics dashboard
- ✅ DataTables integration
- ✅ Image preview
- ✅ Validation (client + server)
- ✅ Loading states
- ✅ Toast notifications

### Not Implemented (Future):
- ⏳ Bulk upload (CSV)
- ⏳ Product variants (SKUs)
- ⏳ Low stock alerts
- ⏳ Sales reports
- ⏳ Promotions/coupons
- ⏳ Shipping integration
- ⏳ Commission calculation

---

## 🧪 TESTING

### Manual Tests:
```bash
# 1. Add Product
Navigate to /tailor/manage/products/add
Fill form, upload images
Submit → Should appear in store

# 2. Edit Product
Click Edit on product
Update details
Submit → Changes reflected

# 3. Update Stock
Enter new quantity
Click check button
→ Stock updated, availability auto-changed if 0

# 4. Toggle Availability
Click switch
→ Product enabled/disabled instantly

# 5. Delete Product
Click delete button
Confirm in modal
→ Product soft-deleted (IsDeleted = true)

# 6. View in Store
Click preview button
→ Opens product details in new tab
```

---

## 📈 ANALYTICS

### Per Product:
```
- View Count (increments on details page view)
- Sales Count (increments on order)
- Average Rating (from reviews)
- Review Count (total reviews)
```

### Dashboard:
```
- Total Products
- Active Products (Available + Stock > 0)
- Out of Stock (Stock = 0)
- Inventory Value (SUM of Price × Stock)
```

---

## 🔄 WORKFLOW

### Product Lifecycle:
```
1. CREATE
   ↓
2. PUBLISHED (IsAvailable = true)
   ↓
3. VISIBLE IN STORE
   ↓
4. CUSTOMERS BUY
   ↓
5. STOCK DECREASES
   ↓
6. OUT OF STOCK (Auto-disabled)
   ↓
7. RESTOCK (Manual)
   ↓
8. AVAILABLE AGAIN
   ↓
9. DELETE (If no active orders)
```

---

## 💡 TIPS

### Best Practices:
1. **Images:** Use high-quality, clear photos
2. **Descriptions:** Detailed, accurate, honest
3. **Pricing:** Competitive, fair discounts
4. **Stock:** Keep updated to avoid overselling
5. **Categories:** Choose most relevant
6. **SEO:** Fill meta fields for better search
7. **Featured:** Highlight best-sellers

### Common Issues:
```
Q: Can't upload image?
A: Check size (<5MB) and format (JPG/PNG/WEBP)

Q: Can't delete product?
A: Product may be in active orders

Q: Stock not updating?
A: Check for JavaScript errors, refresh page

Q: Product not showing in store?
A: Ensure IsAvailable = true and IsDeleted = false
```

---

## 🎓 LEARNING RESOURCES

### Related Documentation:
- `TAILOR_PRODUCT_MANAGEMENT_GUIDE.md` (Full guide)
- `CHECKOUT_MERGE_GUIDE.md` (Store checkout)
- `README.md` (Platform overview)

### Code References:
- `Models/Product.cs` (Data model)
- `Controllers/StoreController.cs` (Store integration)
- `Controllers/TailorManagementController.cs` (Management)

---

## ✅ CHECKLIST

### Before Going Live:
- [ ] Test add product with all fields
- [ ] Test image upload (primary + additional)
- [ ] Test edit product
- [ ] Test stock update
- [ ] Test toggle availability
- [ ] Test delete (with and without orders)
- [ ] Verify products appear in store
- [ ] Test mobile responsive
- [ ] Check authorization (only tailors can access)
- [ ] Verify image serving works
- [ ] Test search/filter in store
- [ ] Check cart integration

---

## 📞 SUPPORT

### Questions?
1. Check full documentation: `TAILOR_PRODUCT_MANAGEMENT_GUIDE.md`
2. Review code comments in controller
3. Test in development environment first
4. Check browser console for errors

---

**Quick Reference Version:** 1.0  
**Last Updated:** 2024-11-22  
**Status:** Ready ✅
