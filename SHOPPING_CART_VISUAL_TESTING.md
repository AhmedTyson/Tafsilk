# ✅ VISUAL TESTING CHECKLIST - SHOPPING CART

## 🎯 **COMPLETE VISUAL VERIFICATION**

Use this checklist to verify every visual element works correctly.

---

## 📋 **TESTING CHECKLIST**

### **✅ STEP 1: PRODUCT LISTING (/Store)**

#### **Layout:**
- [ ] 3-column grid on desktop
- [ ] 2-column grid on tablet
- [ ] 1-column on mobile
- [ ] Cards have consistent height
- [ ] Images load correctly
- [ ] Placeholder shows if no image

#### **Product Cards:**
- [ ] Product name displays
- [ ] Price displays (in SAR)
- [ ] Discount badge shows (if applicable)
- [ ] Stock badge shows (if low stock)
- [ ] Category badge visible
- [ ] Featured badge shows (if featured)
- [ ] Rating stars display correctly
- [ ] Review count shows

#### **Filters:**
- [ ] Category dropdown works
- [ ] Search box accepts input
- [ ] Price range sliders work
- [ ] Sort dropdown changes order
- [ ] "Apply Filters" button works
- [ ] "Reset" button clears filters

#### **Interactions:**
- [ ] Clicking card opens product details
- [ ] Hover effect on cards
- [ ] Smooth transitions
- [ ] Loading states show

---

### **✅ STEP 2: PRODUCT DETAILS (/Store/Product/{id})**

#### **Layout:**
- [ ] 2-column layout (image left, details right)
- [ ] Breadcrumb navigation at top
- [ ] Image takes 50% width on desktop
- [ ] Details take 50% width
- [ ] Stacked on mobile

#### **Product Images:**
- [ ] Main image displays large
- [ ] Thumbnail gallery shows below (if multiple images)
- [ ] Clicking thumbnail changes main image
- [ ] Zoom works on main image click
- [ ] Placeholder shows if no image

#### **Product Info:**
- [ ] Name displays as h1
- [ ] Category badge shows
- [ ] Featured badge shows (if applicable)
- [ ] Stock badge shows correct status
- [ ] Rating stars render correctly
- [ ] Review count displays
- [ ] Price displays prominently
- [ ] Discount shown (if applicable)
- [ ] Savings percentage calculated

#### **Product Details Card:**
- [ ] Light background
- [ ] Description text readable
- [ ] Material shows (if exists)
- [ ] Brand shows (if exists)
- [ ] Color shows (if exists)
- [ ] Size shows (if exists)
- [ ] Stock quantity displays

#### **Add to Cart Form (Customer Only):**
- [ ] Quantity selector shows
- [ ] + button works
- [ ] - button works
- [ ] Manual input works
- [ ] Cannot go below 1
- [ ] Cannot exceed stock
- [ ] Size dropdown shows (if product has sizes)
- [ ] Color input shows (if product has color)
- [ ] Special instructions textarea shows
- [ ] "Add to Cart" button prominent
- [ ] Button disabled if out of stock

#### **Auth States:**
- [ ] Not logged in: Shows login prompt
- [ ] Logged in as Tailor: Shows warning message
- [ ] Logged in as Customer: Shows add to cart form
- [ ] Out of stock: Shows "Out of Stock" alert

#### **Tabs:**
- [ ] Description tab active by default
- [ ] Reviews tab clickable
- [ ] Tab content switches
- [ ] Reviews show placeholder

---

### **✅ STEP 3: SHOPPING CART (/Store/Cart)**

#### **Empty State:**
- [ ] Large cart icon shows
- [ ] "Your cart is empty" message
- [ ] "Start Shopping" button
- [ ] Button links to /Store

#### **Cart with Items:**

**Item Cards:**
- [ ] Product image displays (or placeholder)
- [ ] Product name shows
- [ ] Selected size displays (if chosen)
- [ ] Selected color displays (if chosen)
- [ ] Unit price shows
- [ ] Quantity controls visible
- [ ] + button works
- [ ] - button works
- [ ] Manual input works
- [ ] Cannot go below 1
- [ ] Cannot exceed stock
- [ ] Line total calculates correctly
- [ ] Remove button shows
- [ ] Confirmation popup on remove

**Stock Warnings:**
- [ ] Red badge if product unavailable
- [ ] Yellow badge if quantity > stock
- [ ] Message shows available quantity

**Order Summary (Sidebar):**
- [ ] Sticky on scroll
- [ ] Subtotal displays
- [ ] Item count correct
- [ ] Shipping cost shows
- [ ] "FREE" if ≥ 500 SAR
- [ ] Amount if < 500 SAR
- [ ] Free shipping promo message
- [ ] Tax (15%) displays
- [ ] Grand total prominent
- [ ] "Proceed to Checkout" button
- [ ] Security badge shows

**Actions:**
- [ ] "Continue Shopping" button
- [ ] "Clear Cart" button
- [ ] Confirmation popup on clear
- [ ] Cart badge updates in navbar

**Trust Badges:**
- [ ] Fast shipping icon
- [ ] Easy return icon
- [ ] 24/7 support icon

---

### **✅ STEP 4: CHECKOUT (/Store/Checkout)**

#### **Progress Indicator:**
- [ ] 4 steps shown
- [ ] "Shopping" completed (green check)
- [ ] "Cart" completed (green check)
- [ ] "Payment" current (blue dot)
- [ ] "Confirmation" pending (gray)
- [ ] Progress bar at 75%

#### **Shipping Address Card:**
- [ ] Blue header
- [ ] Shipping icon
- [ ] Full Name field (required)
- [ ] Pre-filled from profile
- [ ] Phone field with +966 prefix
- [ ] Street address field (required)
- [ ] City dropdown (required)
- [ ] Saudi cities listed
- [ ] District field (optional)
- [ ] Postal code field (5 digits)
- [ ] Additional info textarea
- [ ] All required fields marked with *
- [ ] Validation messages show on submit

#### **Payment Method Card:**
- [ ] Green header
- [ ] Money icon
- [ ] Credit/Debit card option
- [ ] Card icon displays
- [ ] Selected by default
- [ ] Cash on Delivery option
- [ ] Cash icon displays
- [ ] Only one selectable
- [ ] Demo notice for credit card

#### **Delivery Notes Card:**
- [ ] Optional textarea
- [ ] Placeholder text
- [ ] Accepts input

#### **Order Summary (Sidebar):**
- [ ] Sticky on scroll
- [ ] Dark header
- [ ] Receipt icon
- [ ] Items list (scrollable)
- [ ] Each item shows:
  - Name
  - Size (if selected)
  - Quantity × Unit Price
  - Line Total
- [ ] Subtotal displays
- [ ] Shipping displays (with FREE badge if 0)
- [ ] Tax displays
- [ ] Grand total prominent
- [ ] "Confirm Order" button (green, large)
- [ ] "Back to Cart" button
- [ ] Terms & conditions checkbox
- [ ] Terms modal link works
- [ ] Security badge

**Trust Badges:**
- [ ] Secure icon
- [ ] Fast delivery icon
- [ ] Free return icon

#### **Form Validation:**
- [ ] Required fields show error if empty
- [ ] Phone number validates format
- [ ] Cannot submit without terms checked
- [ ] Submit button shows loading state
- [ ] Button disabled during submit

#### **Terms Modal:**
- [ ] Opens on link click
- [ ] Shows terms content
- [ ] Close button works
- [ ] Clicking outside closes

---

### **✅ STEP 5: ORDER CONFIRMATION (/Orders/{id})**

#### **Order Details:**
- [ ] Order number displays
- [ ] Status shows "تم التأكيد" (Confirmed)
- [ ] Status badge shows (green/blue)
- [ ] Created date displays
- [ ] Service type shows "StoreOrder"
- [ ] Total price displays
- [ ] Description shows "Store Purchase"

#### **Customer Info:**
- [ ] Customer name shows
- [ ] Customer phone shows (if available)

#### **Tailor Info:**
- [ ] System tailor name shows
- [ ] Tailor shop name shows
- [ ] Profile picture (if exists)

#### **Order Items:**
- [ ] All purchased items listed
- [ ] Product names show
- [ ] Quantities correct
- [ ] Prices correct
- [ ] Sizes show (if selected)
- [ ] Colors show (if selected)

#### **Payment Info:**
- [ ] Payment status shows
- [ ] Payment method displays
- [ ] Amount shows

#### **Images:**
- [ ] Reference images display (if any)

#### **Actions (Customer View):**
- [ ] Can view order
- [ ] Cannot cancel (if already confirmed)

#### **Success Message:**
- [ ] TempData success message shows
- [ ] "Order placed successfully!"

---

## 🎨 **VISUAL CONSISTENCY CHECKS**

### **Typography:**
- [ ] Headers use Arabic fonts
- [ ] Body text readable
- [ ] Consistent font sizes
- [ ] Proper RTL text direction

### **Colors:**
- [ ] Primary: Blue (#0d6efd)
- [ ] Success: Green (#198754)
- [ ] Danger: Red (#dc3545)
- [ ] Warning: Yellow (#ffc107)
- [ ] Consistent throughout

### **Spacing:**
- [ ] Consistent padding in cards
- [ ] Proper margins between sections
- [ ] No overlapping elements
- [ ] Breathing room around content

### **Icons:**
- [ ] FontAwesome icons load
- [ ] Icons have consistent size
- [ ] Icons align with text
- [ ] Proper icon colors

### **Buttons:**
- [ ] Primary buttons blue
- [ ] Success buttons green
- [ ] Danger buttons red
- [ ] Hover effects work
- [ ] Consistent sizing
- [ ] Proper spacing

### **Cards:**
- [ ] Consistent shadow
- [ ] Rounded corners
- [ ] Proper borders
- [ ] Headers styled consistently

---

## 📱 **RESPONSIVE CHECKS**

### **Desktop (≥ 1024px):**
- [ ] 3-column product grid
- [ ] Sidebar sticky
- [ ] Full navigation
- [ ] All features visible

### **Tablet (768px - 1023px):**
- [ ] 2-column product grid
- [ ] Sidebar becomes full width after content
- [ ] Collapsible filters
- [ ] Touch-friendly buttons

### **Mobile (< 768px):**
- [ ] 1-column product grid
- [ ] Stacked layout
- [ ] Mobile menu works
- [ ] Forms stack vertically
- [ ] Larger touch targets
- [ ] Quantity controls accessible

---

## 🔄 **INTERACTION CHECKS**

### **Animations:**
- [ ] Smooth page transitions
- [ ] Hover effects on cards
- [ ] Button hover states
- [ ] Loading spinners
- [ ] Fade in/out effects

### **Form Interactions:**
- [ ] Input focus states
- [ ] Validation feedback
- [ ] Error messages appear
- [ ] Success messages appear
- [ ] Auto-submit on quantity change

### **Navigation:**
- [ ] Breadcrumbs work
- [ ] Back buttons work
- [ ] Links open correctly
- [ ] Redirects work

---

## 🔒 **SECURITY VISUAL CHECKS**

### **Auth Required Pages:**
- [ ] Cart requires login (Customer)
- [ ] Checkout requires login (Customer)
- [ ] Add to cart requires login
- [ ] Proper error messages

### **Role Checks:**
- [ ] Tailors see appropriate message
- [ ] Customers see cart features
- [ ] Guests see login prompts

---

## ✅ **FINAL VERIFICATION**

### **Complete Flow Test:**
```
1. [ ] Open /Store
2. [ ] See 12 products
3. [ ] Click first product
4. [ ] See product details
5. [ ] Login as customer
6. [ ] Select quantity: 2
7. [ ] Select size: M
8. [ ] Add special note
9. [ ] Click "Add to Cart"
10. [ ] See success message
11. [ ] Cart badge shows "2"
12. [ ] Go to cart
13. [ ] See product in cart
14. [ ] Increase quantity to 3
15. [ ] Go back to store
16. [ ] Add different product
17. [ ] Cart badge shows "5"
18. [ ] Go to cart
19. [ ] See 2 products
20. [ ] Remove one product
21. [ ] Confirm removal
22. [ ] See 1 product remaining
23. [ ] Click "Proceed to Checkout"
24. [ ] See pre-filled address
25. [ ] Fill remaining fields
26. [ ] Select payment: COD
27. [ ] Check terms
28. [ ] Click "Confirm Order"
29. [ ] See loading state
30. [ ] Redirect to order page
31. [ ] See order details
32. [ ] Status = Confirmed
33. [ ] All items listed
34. [ ] Go to /Store/Cart
35. [ ] Cart is empty
36. [ ] Success! ✅
```

---

## 🎯 **PASS CRITERIA**

**To pass visual testing:**
- ✅ All checkboxes above checked
- ✅ No visual glitches
- ✅ No layout breaks
- ✅ All features accessible
- ✅ Responsive on all sizes
- ✅ Arabic text displays correctly
- ✅ RTL direction correct
- ✅ Colors consistent
- ✅ Animations smooth

**Current Status: READY FOR TESTING** ✅

---

## 📸 **SCREENSHOT CHECKLIST**

Take screenshots of:
- [ ] Store index (desktop)
- [ ] Store index (mobile)
- [ ] Product details
- [ ] Shopping cart (with items)
- [ ] Shopping cart (empty)
- [ ] Checkout page
- [ ] Order confirmation
- [ ] Cart badge in navbar

---

## 🎉 **VISUAL TESTING COMPLETE!**

Once all checks pass, your shopping cart UI is **production-ready**! 🚀

