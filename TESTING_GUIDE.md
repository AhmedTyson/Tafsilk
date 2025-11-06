# 🧪 Tafsilk Platform - Test Data & Testing Guide

## 📊 Database Seeding Complete

### ✅ **Quick Seed Command**

```bash
# Option 1: Using API Endpoint (Recommended)
# Start the application
dotnet run

# Then call the API endpoint
curl -X POST https://localhost:7186/api/DevData/seed-test-data

# Option 2: Using SQL Script
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -i "Scripts/SeedTestData.sql" -I
```

---

## 🔑 **Test Credentials**

**Password for ALL test accounts:** `Test@123`

### **Customer Accounts:**
1. **ahmed.hassan@tafsilk.test**
   - Tier: Bronze (250 points)
  - Has saved measurements
   - 1 pending order

2. **fatima.ali@tafsilk.test**
  - Tier: Bronze (150 points)
   - 1 confirmed order (with deposit paid)

3. **mohamed.salem@tafsilk.test**
   - Tier: Bronze (100 points)
   - 1 completed order

### **Tailor Accounts:**
1. **master.tailor@tafsilk.test**
   - Name: Master Ibrahim
   - Shop: Al-Fakhama Tailoring
   - Location: Cairo, Nasr City
   - Specialization: Men's Suits & Thobes
   - Rating: 4.8/5
   - Services: 2 (Business Suits, Thobes)

2. **wedding.specialist@tafsilk.test**
   - Name: Madame Laila
   - Shop: Elegance Bridal Studio
   - Location: Alexandria, Smouha
 - Specialization: Wedding & Evening Dresses
   - Rating: 4.9/5
   - Services: 2 (Wedding Dress, Evening Gown)

---

## 🎯 **Test Scenarios**

### **Scenario 1: Customer Registration & Login**
1. Navigate to `https://localhost:7186/Account/Register`
2. Create new customer account OR login with test account
3. Verify auto-login and redirect to customer dashboard
4. Check loyalty record created automatically

### **Scenario 2: Browse Tailors**
1. Login as customer
2. Navigate to "Search Tailors" or `/Tailors`
3. View tailor profiles
4. Check ratings, services, portfolio
5. Filter by city/specialization

### **Scenario 3: Create Order (Full Workflow)**

**Step 1: Select Tailor & Service**
1. Login: `ahmed.hassan@tafsilk.test` / `Test@123`
2. Go to "Search Tailors"
3. Click on "Master Ibrahim"
4. Click "Book Now" or navigate to `/Orders/Create/{tailorId}`

**Step 2: Order Details**
- Service: Custom Business Suit
- Description: "Custom navy suit for wedding"
- Reference images: Upload 1-2 photos (optional)
- Measurements: Use saved measurements OR enter manually
- Due date: 14 days from now

**Step 3: Review & Submit**
- Check VAT calculation (15%)
- Check deposit requirement (50%)
- Confirm booking

**Expected Result:**
- Order created with status: `QuotePending`
- Customer redirected to order details page
- Notification sent (check Notifications table)

### **Scenario 4: Tailor Quote Workflow**
1. Login as tailor: `master.tailor@tafsilk.test` / `Test@123`
2. Go to "My Orders" or `/Orders/Tailor/Manage`
3. View pending orders (QuotePending status)
4. Provide quote:
 - Set `TailorQuote` amount
   - Add `TailorQuoteNotes`
 - Update status to `Confirmed`
5. Customer receives notification

### **Scenario 5: Deposit Payment**
1. Login as customer
2. View confirmed order
3. Make deposit payment:
   - Payment type: VodafoneCash/Card/Wallet
   - Amount: 50% of total
   - Transaction type: Deposit
4. Update order: `DepositPaid = true`
5. Order status → `InProgress`

### **Scenario 6: Saved Measurements**
1. Login: `ahmed.hassan@tafsilk.test`
2. Go to Profile → Saved Measurements
3. View existing "My Default Thobe" measurement
4. Create new order and select saved measurements
5. **Result:** 60-70% faster booking!

### **Scenario 7: Loyalty Points**
1. Login as any customer
2. Complete an order
3. Check loyalty points updated:
 - Points earned based on order value
   - Tier upgrade if threshold reached
4. View transaction history
5. Redeem points for discount

### **Scenario 8: Submit Complaint**
1. Login as customer with completed order
2. Navigate to order details
3. Click "Submit Complaint"
4. Fill form:
   - Type: Quality/Delay/Communication
   - Description: Issue details
   - Upload evidence photos
   - Desired resolution: Refund/Rework/Apology
5. Submit and track status

### **Scenario 9: Order Completion Flow**
**Tailor Side:**
1. Update order status: `InProgress` → `ReadyForPickup`
2. Notify customer

**Customer Side:**
1. Receive notification
2. Pickup order (or request delivery)
3. Inspect quality
4. Make final payment (remaining 50%)
5. Confirm completion

**Result:**
- Order status → `Completed`
- Review prompt displayed

### **Scenario 10: Leave Review**
1. After order completion
2. Customer redirected to review page
3. Rate overall (1-5 stars)
4. Rate dimensions:
   - Quality
   - Communication
   - Timeliness
   - Pricing
5. Write comment
6. Upload before/after photos (optional)
7. Submit review
8. Tailor's rating updated automatically

---

## 📋 **Test Data Summary**

| Entity | Count | Notes |
|--------|-------|-------|
| **Users** | 5 | 3 customers + 2 tailors |
| **Customer Profiles** | 3 | With loyalty records |
| **Tailor Profiles** | 2 | Both verified |
| **Loyalty Records** | 3 | Bronze tier |
| **Saved Measurements** | 1 | Ahmed's thobe size |
| **Tailor Services** | 4 | 2 per tailor |
| **Orders** | 3 | Different statuses |
| **Payments** | 0 | To be created during testing |
| **Reviews** | 0 | To be created |
| **Complaints** | 0 | To be created |

---

## 🔍 **Verification Queries**

### **Check Test Data Exists:**
```sql
USE TafsilkPlatformDb_Dev;

-- Count test users
SELECT COUNT(*) FROM Users WHERE Email LIKE '%@tafsilk.test';

-- View test customers
SELECT u.Email, cp.FullName, cl.Points, cl.Tier
FROM Users u
JOIN CustomerProfiles cp ON u.Id = cp.UserId
LEFT JOIN CustomerLoyalty cl ON cp.Id = cl.CustomerId
WHERE u.Email LIKE '%@tafsilk.test';

-- View test tailors
SELECT u.Email, tp.FullName, tp.ShopName, tp.City, tp.AverageRating
FROM Users u
JOIN TailorProfiles tp ON u.Id = tp.UserId
WHERE u.Email LIKE '%@tafsilk.test';

-- View test orders
SELECT 
    o.OrderId,
    CASE o.Status
     WHEN 0 THEN 'QuotePending'
    WHEN 1 THEN 'Confirmed'
        WHEN 2 THEN 'InProgress'
      WHEN 3 THEN 'ReadyForPickup'
 WHEN 4 THEN 'Completed'
   WHEN 5 THEN 'Cancelled'
    END AS Status,
    o.TotalPrice,
    o.RequiresDeposit,
    o.DepositPaid,
    cp.FullName AS Customer,
    tp.ShopName AS Tailor
FROM Orders o
JOIN CustomerProfiles cp ON o.CustomerId = cp.Id
JOIN TailorProfiles tp ON o.TailorId = tp.Id;
```

---

## 🧹 **Clear Test Data**

### **Option 1: Using API**
```bash
curl -X DELETE https://localhost:7186/api/DevData/clear-test-data
```

### **Option 2: Using SQL**
```sql
-- Delete in correct order
DELETE FROM LoyaltyTransactions WHERE CustomerLoyaltyId IN (SELECT Id FROM CustomerLoyalty WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test')));
DELETE FROM CustomerLoyalty WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM CustomerMeasurements WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM Complaints WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM Payment WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM Orders WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM Reviews WHERE CustomerId IN (SELECT Id FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM PortfolioImages WHERE TailorId IN (SELECT Id FROM TailorProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM TailorServices WHERE TailorId IN (SELECT Id FROM TailorProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test'));
DELETE FROM TailorProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test');
DELETE FROM CustomerProfiles WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@tafsilk.test');
DELETE FROM Users WHERE Email LIKE '%@tafsilk.test';
```

---

## ✅ **Testing Checklist**

### **Authentication:**
- [ ] Register new customer
- [ ] Login with test account
- [ ] Google OAuth (if configured)
- [ ] Facebook OAuth (if configured)
- [ ] Password reset
- [ ] Email verification

### **Customer Features:**
- [ ] View customer dashboard
- [ ] Browse tailors
- [ ] View tailor profile
- [ ] Create order (6 steps)
- [ ] Use saved measurements
- [ ] Pay deposit
- [ ] Track order status
- [ ] Submit complaint
- [ ] Leave review
- [ ] Check loyalty points
- [ ] Redeem rewards

### **Tailor Features:**
- [ ] View tailor dashboard
- [ ] Manage services
- [ ] View orders
- [ ] Provide quote
- [ ] Update order status
- [ ] View reviews
- [ ] Manage portfolio

### **Order Workflow:**
- [ ] QuotePending → Confirmed
- [ ] Confirmed → InProgress (after deposit)
- [ ] InProgress → ReadyForPickup
- [ ] ReadyForPickup → Completed
- [ ] Cancelled status

### **Payment System:**
- [ ] Cash on Delivery
- [ ] Card payment
- [ ] Mobile wallet (VodafoneCash)
- [ ] Deposit payment
- [ ] Final payment
- [ ] Payment tracking

### **Loyalty System:**
- [ ] Points earned on order
- [ ] Tier upgrade
- [ ] Referral code generation
- [ ] Points redemption
- [ ] Transaction history

---

## 🎉 **Success Metrics**

**You've successfully tested when:**
1. ✅ Can login with test accounts
2. ✅ Customer can create order through full workflow
3. ✅ Tailor can provide quote and update status
4. ✅ Deposit payment records correctly
5. ✅ Saved measurements work in booking
6. ✅ Loyalty points accumulate
7. ✅ Reviews can be submitted
8. ✅ Complaints can be filed
9. ✅ All order statuses transition correctly
10. ✅ No errors in console or logs

---

## 🚀 **Next Steps After Testing**

1. **Build Real UI Components:**
   - 6-step booking wizard
   - Loyalty dashboard
   - Complaint submission form
   - Enhanced search with GPS

2. **Implement Service Layer:**
   - LoyaltyService for points calculation
   - ComplaintService for workflow
   - BookingService for wizard logic
   - NotificationService for alerts

3. **Add Real Payment Integration:**
   - Vodafone Cash API
   - Orange Cash API
   - Etisalat Cash API
   - Card payment gateway

4. **Enhance Features:**
   - Real-time notifications
   - SMS alerts
   - Email templates
   - Push notifications

---

**Happy Testing! 🎉**

For issues or questions, check the logs in `/Logs/` or documentation in:
- `QUICK_START_GUIDE.md`
- `MIGRATION_STATUS_REPORT.md`
- `CUSTOMER_JOURNEY_IMPLEMENTATION_SUMMARY.md`
