# Database Migration Guide - Customer Journey Implementation

## 📋 Overview
This guide walks you through applying the database changes for the enhanced customer journey workflow implementation.

---

## ⚠️ **IMPORTANT: Backup Your Database First!**

Before running any migrations, **ALWAYS backup your production database**:

```sql
-- SQL Server Backup Command
BACKUP DATABASE [TafsilkPlatform] 
TO DISK = 'C:\Backups\TafsilkPlatform_BeforeMigration_20250120.bak'
WITH FORMAT, COMPRESSION;
```

---

## 🔄 **Step 1: Create the Migration**

Open a terminal in the project directory and run:

```bash
dotnet ef migrations add AddLoyaltyComplaintsAndMeasurements --project TafsilkPlatform.Web
```

This will create a new migration file with the following changes:
- **New Tables**: CustomerLoyalty, LoyaltyTransactions, CustomerMeasurements, Complaints, ComplaintAttachments
- **Modified Tables**: Orders (added quote and deposit fields)
- **Updated Enums**: OrderStatus, PaymentType, TransactionType, PaymentStatus

---

## 🗄️ **Step 2: Review the Migration**

Before applying, review the generated migration file in:
```
TafsilkPlatform.Web/Migrations/XXXXXX_AddLoyaltyComplaintsAndMeasurements.cs
```

Expected changes:
1. **Create CustomerLoyalty table**
2. **Create LoyaltyTransactions table**
3. **Create CustomerMeasurements table**
4. **Create Complaints table**
5. **Create ComplaintAttachments table**
6. **Alter Orders table** (add new columns)

---

## ▶️ **Step 3: Apply the Migration**

### Development Environment:
```bash
dotnet ef database update --project TafsilkPlatform.Web
```

### Production Environment:
```bash
# Generate SQL script for review
dotnet ef migrations script --project TafsilkPlatform.Web --output migration_script.sql

# Review the script, then apply manually or:
dotnet ef database update --project TafsilkPlatform.Web --connection "YourProductionConnectionString"
```

---

## ✅ **Step 4: Verify the Migration**

After applying the migration, verify the changes:

```sql
-- Check new tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'CustomerLoyalty',
    'LoyaltyTransactions',
    'CustomerMeasurements',
    'Complaints',
    'ComplaintAttachments'
);

-- Check Orders table has new columns
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' 
AND COLUMN_NAME IN (
    'TailorQuote',
    'TailorQuoteNotes',
    'QuoteProvidedAt',
    'RequiresDeposit',
    'DepositAmount',
    'DepositPaid',
    'DepositPaidAt',
    'MeasurementsJson',
    'FulfillmentMethod',
    'DeliveryAddress'
);

-- Verify indexes
EXEC sp_helpindex 'CustomerLoyalty';
EXEC sp_helpindex 'Complaints';
```

---

## 🔄 **Step 5: Update Existing Data (Optional)**

If you have existing orders, you may want to initialize new fields:

```sql
-- Set default values for existing orders
UPDATE Orders
SET 
    RequiresDeposit = 0,
    DepositPaid = 0,
    FulfillmentMethod = 'Pickup'
WHERE RequiresDeposit IS NULL;

-- Initialize loyalty records for existing customers
INSERT INTO CustomerLoyalty (Id, CustomerId, Points, LifetimePoints, Tier, TotalOrders, ReferralsCount, CreatedAt)
SELECT 
    NEWID(),
    cp.Id,
    0,
    0,
    'Bronze',
    (SELECT COUNT(*) FROM Orders WHERE CustomerId = cp.Id),
    0,
    GETUTCDATE()
FROM CustomerProfiles cp
WHERE NOT EXISTS (SELECT 1 FROM CustomerLoyalty WHERE CustomerId = cp.Id);
```

---

## 📊 **New Database Schema**

### **CustomerLoyalty Table**
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| CustomerId | GUID | FK to CustomerProfiles |
| Points | INT | Current points balance |
| LifetimePoints | INT | Total points earned ever |
| Tier | NVARCHAR(50) | Bronze/Silver/Gold/Platinum |
| TotalOrders | INT | Completed orders count |
| ReferralsCount | INT | Number of referrals made |
| ReferralCode | NVARCHAR(20) | Unique referral code |
| ReferredBy | GUID | Who referred this customer |
| CreatedAt | DATETIME | Record creation timestamp |

### **LoyaltyTransactions Table**
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| CustomerLoyaltyId | GUID | FK to CustomerLoyalty |
| Points | INT | Points (+/-) |
| Type | NVARCHAR(20) | Earned/Redeemed/Expired/Bonus |
| Description | NVARCHAR(200) | Transaction description |
| RelatedOrderId | GUID | Associated order (nullable) |
| CreatedAt | DATETIME | Transaction timestamp |

### **CustomerMeasurements Table**
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| CustomerId | GUID | FK to CustomerProfiles |
| Name | NVARCHAR(100) | Measurement set name |
| GarmentType | NVARCHAR(50) | Thobe/Suit/Dress/Abaya |
| Chest | DECIMAL(5,2) | Chest measurement (cm) |
| Waist | DECIMAL(5,2) | Waist measurement (cm) |
| Hips | DECIMAL(5,2) | Hips measurement (cm) |
| ... | ... | Other body measurements |
| CustomMeasurementsJson | NVARCHAR(2000) | Additional custom data |
| Notes | NVARCHAR(500) | Customer notes |
| IsDefault | BIT | Default measurement set |
| CreatedAt | DATETIME | Creation timestamp |

### **Complaints Table**
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| OrderId | GUID | FK to Orders |
| CustomerId | GUID | FK to CustomerProfiles |
| TailorId | GUID | FK to TailorProfiles |
| Subject | NVARCHAR(100) | Complaint title |
| Description | NVARCHAR(2000) | Complaint details |
| ComplaintType | NVARCHAR(50) | Quality/Delay/Communication |
| DesiredResolution | NVARCHAR(50) | Refund/Rework/PartialRefund |
| Status | NVARCHAR(50) | Open/UnderReview/Resolved |
| Priority | NVARCHAR(20) | Low/Medium/High/Critical |
| AdminResponse | NVARCHAR(2000) | Admin's response |
| ResolvedBy | GUID | Admin who resolved (nullable) |
| CreatedAt | DATETIME | Complaint timestamp |
| UpdatedAt | DATETIME | Last update timestamp |
| ResolvedAt | DATETIME | Resolution timestamp |

### **ComplaintAttachments Table**
| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| ComplaintId | GUID | FK to Complaints |
| FileData | VARBINARY(MAX) | Evidence file binary |
| ContentType | NVARCHAR(100) | MIME type |
| FileName | NVARCHAR(255) | Original filename |
| UploadedAt | DATETIME | Upload timestamp |

### **Orders Table (New Columns)**
| Column | Type | Description |
|--------|------|-------------|
| TailorQuote | FLOAT | Tailor's quoted price |
| TailorQuoteNotes | NVARCHAR(MAX) | Quote details |
| QuoteProvidedAt | DATETIMEOFFSET | Quote timestamp |
| RequiresDeposit | BIT | Deposit required flag |
| DepositAmount | FLOAT | Deposit amount |
| DepositPaid | BIT | Deposit paid flag |
| DepositPaidAt | DATETIMEOFFSET | Deposit payment timestamp |
| MeasurementsJson | NVARCHAR(MAX) | Customer measurements JSON |
| FulfillmentMethod | NVARCHAR(20) | Pickup/Delivery |
| DeliveryAddress | NVARCHAR(MAX) | Delivery address details |

---

## 🔍 **Troubleshooting**

### **Migration Fails with Foreign Key Error:**
```sql
-- Temporarily disable constraints
ALTER TABLE Orders NOCHECK CONSTRAINT ALL;
-- Run migration
-- Re-enable constraints
ALTER TABLE Orders CHECK CONSTRAINT ALL;
```

### **Rollback Migration:**
```bash
# List migrations
dotnet ef migrations list --project TafsilkPlatform.Web

# Rollback to previous
dotnet ef database update PreviousMigrationName --project TafsilkPlatform.Web

# Remove migration file
dotnet ef migrations remove --project TafsilkPlatform.Web
```

### **Check Migration Status:**
```bash
dotnet ef migrations list --project TafsilkPlatform.Web
```

---

## 📈 **Performance Optimization**

After migration, create additional indexes for performance:

```sql
-- Loyalty queries optimization
CREATE NONCLUSTERED INDEX IX_CustomerLoyalty_Tier 
ON CustomerLoyalty(Tier) INCLUDE (Points, LifetimePoints);

-- Measurements search optimization
CREATE NONCLUSTERED INDEX IX_CustomerMeasurements_GarmentType 
ON CustomerMeasurements(CustomerId, GarmentType) 
WHERE IsDefault = 1;

-- Complaints reporting optimization
CREATE NONCLUSTERED INDEX IX_Complaints_StatusCreatedAt 
ON Complaints(Status, CreatedAt DESC) 
INCLUDE (ComplaintType, Priority);

-- Orders fulfillment optimization
CREATE NONCLUSTERED INDEX IX_Orders_FulfillmentStatus 
ON Orders(FulfillmentMethod, Status) 
INCLUDE (TotalPrice, DepositPaid);
```

---

## ✅ **Post-Migration Checklist**

- [ ] Backup completed successfully
- [ ] Migration script reviewed
- [ ] Migration applied without errors
- [ ] All new tables created
- [ ] Existing data intact
- [ ] Indexes created
- [ ] Foreign keys working
- [ ] Application starts without errors
- [ ] Basic CRUD operations tested
- [ ] Performance acceptable

---

## 📞 **Support**

If you encounter issues:
1. Check the error log in `/Logs/`
2. Review migration script: `migration_script.sql`
3. Test in development first
4. Contact support with error details

---

**Last Updated:** 2025-01-20  
**Database Version:** After AddLoyaltyComplaintsAndMeasurements migration
