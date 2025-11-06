-- ========================================
-- Tafsilk Platform - Test Data Seeding Script
-- Customer Journey Workflow Testing
-- ========================================

USE TafsilkPlatformDb_Dev;
GO

-- Set proper options for indexed views and computed columns
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'TAFSILK PLATFORM - TEST DATA SEEDING';
PRINT '========================================';
PRINT '';

-- ========================================
-- STEP 1: Create Test Roles (if not exist)
-- ========================================
PRINT '1. Creating Roles...';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Customer')
BEGIN
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
    VALUES (NEWID(), 'Customer', 'Platform Customer', 1, GETUTCDATE());
    PRINT '   ✅ Customer role created';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Tailor')
BEGIN
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
    VALUES (NEWID(), 'Tailor', 'Platform Tailor', 2, GETUTCDATE());
  PRINT '   ✅ Tailor role created';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
    VALUES (NEWID(), 'Admin', 'Platform Administrator', 3, GETUTCDATE());
    PRINT '   ✅ Admin role created';
END

DECLARE @CustomerRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Customer');
DECLARE @TailorRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Tailor');
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Admin');

PRINT '';

-- ========================================
-- STEP 2: Create Test Users
-- ========================================
PRINT '2. Creating Test Users...';

-- Password: Test@123 (hashed)
DECLARE @PasswordHash NVARCHAR(MAX) = '$2a$11$rK8qPJxPZKxJ8LHKkJXYZeYxLkYhZzQwZYHQhJZYZzQwZYHQhJZYZ';

-- Test Customers (5 users)
DECLARE @Customer1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (Id, Email, PhoneNumber, PasswordHash, RoleId, IsActive, EmailVerified, CreatedAt)
VALUES 
    (@Customer1Id, 'ahmed.hassan@tafsilk.test', '+201012345671', @PasswordHash, @CustomerRoleId, 1, 1, GETUTCDATE()),
    (@Customer2Id, 'fatima.ali@tafsilk.test', '+201012345672', @PasswordHash, @CustomerRoleId, 1, 1, GETUTCDATE()),
    (@Customer3Id, 'mohamed.salem@tafsilk.test', '+201012345673', @PasswordHash, @CustomerRoleId, 1, 1, GETUTCDATE()),
    (@Customer4Id, 'sarah.khaled@tafsilk.test', '+201012345674', @PasswordHash, @CustomerRoleId, 1, 1, GETUTCDATE()),
    (@Customer5Id, 'omar.youssef@tafsilk.test', '+201012345675', @PasswordHash, @CustomerRoleId, 1, 1, GETUTCDATE());

PRINT '   ✅ 5 test customers created';

-- Test Tailors (3 users)
DECLARE @Tailor1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tailor2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tailor3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (Id, Email, PhoneNumber, PasswordHash, RoleId, IsActive, EmailVerified, CreatedAt)
VALUES 
    (@Tailor1Id, 'master.tailor@tafsilk.test', '+201112345671', @PasswordHash, @TailorRoleId, 1, 1, GETUTCDATE()),
    (@Tailor2Id, 'wedding.specialist@tafsilk.test', '+201112345672', @PasswordHash, @TailorRoleId, 1, 1, GETUTCDATE()),
    (@Tailor3Id, 'traditional.expert@tafsilk.test', '+201112345673', @PasswordHash, @TailorRoleId, 1, 1, GETUTCDATE());

PRINT '   ✅ 3 test tailors created';

PRINT '';

-- ========================================
-- STEP 3: Create Customer Profiles
-- ========================================
PRINT '3. Creating Customer Profiles...';

DECLARE @CustomerProfile1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerProfile2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerProfile3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerProfile4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerProfile5Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO CustomerProfiles (Id, UserId, FullName, Gender, City, Bio, CreatedAt)
VALUES 
    (@CustomerProfile1Id, @Customer1Id, 'Ahmed Hassan', 'Male', 'Cairo', 'Regular customer, loves custom thobes', GETUTCDATE()),
    (@CustomerProfile2Id, @Customer2Id, 'Fatima Ali', 'Female', 'Alexandria', 'Wedding dress enthusiast', GETUTCDATE()),
    (@CustomerProfile3Id, @Customer3Id, 'Mohamed Salem', 'Male', 'Giza', 'Business suits specialist', GETUTCDATE()),
    (@CustomerProfile4Id, @Customer4Id, 'Sarah Khaled', 'Female', 'Cairo', 'Fashion forward, loves abayas', GETUTCDATE()),
    (@CustomerProfile5Id, @Customer5Id, 'Omar Youssef', 'Male', 'Mansoura', 'Traditional garments lover', GETUTCDATE());

PRINT '   ✅ 5 customer profiles created';
PRINT '';

-- ========================================
-- STEP 4: Create Tailor Profiles
-- ========================================
PRINT '4. Creating Tailor Profiles...';

DECLARE @TailorProfile1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TailorProfile2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @TailorProfile3Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO TailorProfiles (Id, UserId, FullName, ShopName, City, District, Address, Bio, Specialization, ExperienceYears, IsVerified, AverageRating, CreatedAt, Latitude, Longitude)
VALUES 
    (@TailorProfile1Id, @Tailor1Id, 'Master Ibrahim', 'Al-Fakhama Tailoring', 'Cairo', 'Nasr City', '15 Makram Ebeid St, Nasr City', 'Premium custom tailoring with 20 years experience', 'Men''s Suits & Thobes', 20, 1, 4.8, GETUTCDATE(), 30.0444, 31.2357),
    (@TailorProfile2Id, @Tailor2Id, 'Madame Laila', 'Elegance Bridal Studio', 'Alexandria', 'Smouha', '45 El-Horreya Road, Smouha', 'Specialized in wedding dresses and evening gowns', 'Wedding & Evening Dresses', 15, 1, 4.9, GETUTCDATE(), 31.2001, 29.9187),
    (@TailorProfile3Id, @Tailor3Id, 'Sheikh Abdullah', 'Heritage Garments', 'Giza', 'Dokki', '12 Tahrir St, Dokki', 'Expert in traditional Egyptian and Gulf garments', 'Traditional & Islamic Clothing', 25, 1, 4.7, GETUTCDATE(), 30.0389, 31.2096);

PRINT '   ✅ 3 tailor profiles created';
PRINT '';

-- ========================================
-- STEP 5: Create Tailor Services
-- ========================================
PRINT '5. Creating Tailor Services...';

-- Services for Master Ibrahim
INSERT INTO TailorServices (TailorServiceId, TailorId, ServiceName, Description, BasePrice, EstimatedDuration, IsDeleted)
VALUES 
    (NEWID(), @TailorProfile1Id, 'Custom Business Suit', '3-piece custom business suit with premium fabric', 2500.00, 14, 0),
    (NEWID(), @TailorProfile1Id, 'Traditional Thobe', 'Classic Saudi/Emirati style thobe', 800.00, 7, 0),
    (NEWID(), @TailorProfile1Id, 'Suit Alterations', 'Professional alterations for existing suits', 300.00, 3, 0),
    (NEWID(), @TailorProfile1Id, 'Wedding Suit', 'Premium wedding suit with embroidery options', 3500.00, 21, 0);

-- Services for Madame Laila
INSERT INTO TailorServices (TailorServiceId, TailorId, ServiceName, Description, BasePrice, EstimatedDuration, IsDeleted)
VALUES 
    (NEWID(), @TailorProfile2Id, 'Wedding Dress', 'Custom wedding dress with full embellishments', 8000.00, 45, 0),
    (NEWID(), @TailorProfile2Id, 'Evening Gown', 'Elegant evening gown for special occasions', 3500.00, 14, 0),
    (NEWID(), @TailorProfile2Id, 'Bridesmaid Dress', 'Matching bridesmaid dresses', 2000.00, 10, 0),
    (NEWID(), @TailorProfile2Id, 'Dress Alterations', 'Alterations for wedding/evening dresses', 500.00, 5, 0);

-- Services for Sheikh Abdullah
INSERT INTO TailorServices (TailorServiceId, TailorId, ServiceName, Description, BasePrice, EstimatedDuration, IsDeleted)
VALUES 
    (NEWID(), @TailorProfile3Id, 'Traditional Kaftan', 'Moroccan/Egyptian style kaftan', 1200.00, 10, 0),
    (NEWID(), @TailorProfile3Id, 'Embroidered Abaya', 'Premium abaya with custom embroidery', 1500.00, 14, 0),
    (NEWID(), @TailorProfile3Id, 'Jalabiya', 'Classic Egyptian galabiya', 600.00, 5, 0),
    (NEWID(), @TailorProfile3Id, 'Islamic Prayer Garment', 'Traditional prayer thobe', 700.00, 7, 0);

PRINT '   ✅ 12 tailor services created';
PRINT '';

-- ========================================
-- STEP 6: Create Customer Loyalty Records
-- ========================================
PRINT '6. Creating Loyalty Records...';

INSERT INTO CustomerLoyalty (Id, CustomerId, Points, LifetimePoints, Tier, TotalOrders, ReferralsCount, ReferralCode, CreatedAt)
VALUES 
    (NEWID(), @CustomerProfile1Id, 250, 1250, 'Silver', 5, 2, 'REF' + LEFT(CAST(@CustomerProfile1Id AS NVARCHAR(36)), 8), GETUTCDATE()),
    (NEWID(), @CustomerProfile2Id, 150, 450, 'Bronze', 2, 1, 'REF' + LEFT(CAST(@CustomerProfile2Id AS NVARCHAR(36)), 8), GETUTCDATE()),
    (NEWID(), @CustomerProfile3Id, 500, 3200, 'Gold', 8, 3, 'REF' + LEFT(CAST(@CustomerProfile3Id AS NVARCHAR(36)), 8), GETUTCDATE()),
    (NEWID(), @CustomerProfile4Id, 100, 300, 'Bronze', 1, 0, 'REF' + LEFT(CAST(@CustomerProfile4Id AS NVARCHAR(36)), 8), GETUTCDATE()),
  (NEWID(), @CustomerProfile5Id, 1200, 6500, 'Platinum', 15, 5, 'REF' + LEFT(CAST(@CustomerProfile5Id AS NVARCHAR(36)), 8), GETUTCDATE());

PRINT '   ✅ 5 loyalty records created';
PRINT '';

-- ========================================
-- STEP 7: Create Saved Measurements
-- ========================================
PRINT '7. Creating Saved Measurements...';

-- Ahmed's measurements
INSERT INTO CustomerMeasurements (Id, CustomerId, Name, GarmentType, Chest, Waist, Hips, ShoulderWidth, SleeveLength, InseamLength, NeckCircumference, ThobeLength, IsDefault, CreatedAt)
VALUES 
    (NEWID(), @CustomerProfile1Id, 'My Default Thobe', 'Thobe', 42, 34, 38, 18, 65, 32, 16, 150, 1, GETUTCDATE()),
    (NEWID(), @CustomerProfile1Id, 'Wedding Suit', 'Suit', 42, 34, 38, 18, 65, 32, 16, NULL, 0, GETUTCDATE());

-- Mohamed's measurements
INSERT INTO CustomerMeasurements (Id, CustomerId, Name, GarmentType, Chest, Waist, Hips, ShoulderWidth, SleeveLength, InseamLength, NeckCircumference, IsDefault, CreatedAt)
VALUES 
    (NEWID(), @CustomerProfile3Id, 'Business Suit Standard', 'Suit', 44, 36, 40, 19, 66, 33, 17, 1, GETUTCDATE());

-- Fatima's measurements
INSERT INTO CustomerMeasurements (Id, CustomerId, Name, GarmentType, Chest, Waist, Hips, AbayaLength, Notes, IsDefault, CreatedAt)
VALUES 
    (NEWID(), @CustomerProfile2Id, 'Evening Dress Standard', 'Dress', 36, 28, 38, NULL, 'Prefer A-line style', 1, GETUTCDATE());

-- Sarah's measurements
INSERT INTO CustomerMeasurements (Id, CustomerId, Name, GarmentType, Chest, Waist, Hips, AbayaLength, IsDefault, CreatedAt)
VALUES 
    (NEWID(), @CustomerProfile4Id, 'My Abaya Size', 'Abaya', 38, 30, 40, 145, 1, GETUTCDATE());

PRINT '   ✅ 5 saved measurement sets created';
PRINT '';

-- ========================================
-- STEP 8: Create Test Orders (Full Workflow)
-- ========================================
PRINT '8. Creating Test Orders...';

DECLARE @Order1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Order7Id UNIQUEIDENTIFIER = NEWID();

-- Order 1: QuotePending (New order waiting for tailor quote)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, CreatedAt, DueDate, RequiresDeposit, FulfillmentMethod, MeasurementsJson)
VALUES 
    (@Order1Id, @CustomerProfile1Id, @TailorProfile1Id, 'Custom business suit for job interview', 'Custom Business Suit', 0, 2500.00, GETUTCDATE(), DATEADD(day, 14, GETUTCDATE()), 1, 'Pickup', '{"chest":42,"waist":34,"hips":38}');

-- Order 2: Confirmed (Tailor provided quote, customer confirmed)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, TailorQuote, TailorQuoteNotes, QuoteProvidedAt, RequiresDeposit, DepositAmount, DepositPaid, DepositPaidAt, CreatedAt, DueDate, FulfillmentMethod, MeasurementsJson)
VALUES 
    (@Order2Id, @CustomerProfile2Id, @TailorProfile2Id, 'Wedding dress with lace details', 'Wedding Dress', 1, 8000.00, 8500.00, 'Includes premium French lace and beading', DATEADD(hour, -2, GETUTCDATE()), 1, 4250.00, 1, DATEADD(hour, -1, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE()), DATEADD(day, 28, GETUTCDATE()), 'Delivery', '{"chest":36,"waist":28,"hips":38}');

-- Order 3: InProgress (Tailor working on it)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, TailorQuote, QuoteProvidedAt, RequiresDeposit, DepositAmount, DepositPaid, DepositPaidAt, CreatedAt, DueDate, FulfillmentMethod)
VALUES 
    (@Order3Id, @CustomerProfile3Id, @TailorProfile1Id, '3-piece navy blue suit', 'Custom Business Suit', 2, 2800.00, 2800.00, DATEADD(day, -3, GETUTCDATE()), 1, 1400.00, 1, DATEADD(day, -2, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 'Pickup');

-- Order 4: ReadyForPickup (Completed, awaiting customer pickup)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, TailorQuote, QuoteProvidedAt, RequiresDeposit, DepositAmount, DepositPaid, DepositPaidAt, CreatedAt, DueDate, FulfillmentMethod)
VALUES 
    (@Order4Id, @CustomerProfile4Id, @TailorProfile3Id, 'Black embroidered abaya', 'Embroidered Abaya', 3, 1500.00, 1500.00, DATEADD(day, -10, GETUTCDATE()), 1, 750.00, 1, DATEADD(day, -9, GETUTCDATE()), DATEADD(day, -12, GETUTCDATE()), DATEADD(day, 1, GETUTCDATE()), 'Pickup');

-- Order 5: Completed (Finished and delivered)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, TailorQuote, QuoteProvidedAt, RequiresDeposit, DepositAmount, DepositPaid, DepositPaidAt, CreatedAt, DueDate, FulfillmentMethod)
VALUES 
    (@Order5Id, @CustomerProfile5Id, @TailorProfile3Id, 'White traditional thobe', 'Traditional Thobe', 4, 800.00, 800.00, DATEADD(day, -20, GETUTCDATE()), 0, 0, 0, NULL, DATEADD(day, -22, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE()), 'Pickup');

-- Order 6: Completed (Another finished order)
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, CreatedAt, DueDate, FulfillmentMethod)
VALUES 
    (@Order6Id, @CustomerProfile1Id, @TailorProfile1Id, 'Suit alterations - waist adjustment', 'Suit Alterations', 4, 300.00, DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -25, GETUTCDATE()), 'Pickup');

-- Order 7: Cancelled
INSERT INTO Orders (OrderId, CustomerId, TailorId, Description, OrderType, Status, TotalPrice, CreatedAt, DueDate, FulfillmentMethod)
VALUES 
    (@Order7Id, @CustomerProfile2Id, @TailorProfile2Id, 'Evening gown - customer changed mind', 'Evening Gown', 5, 3500.00, DATEADD(day, -15, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), 'Delivery');

PRINT '   ✅ 7 test orders created (covering all statuses)';
PRINT '';

-- ========================================
-- STEP 9: Create Payments
-- ========================================
PRINT '9. Creating Payment Records...';

-- Payment for Order 2 (Deposit)
INSERT INTO Payment (PaymentId, OrderId, CustomerId, TailorId, Amount, PaymentType, PaymentStatus, TransactionType, PaidAt)
VALUES 
    (NEWID(), @Order2Id, @CustomerProfile2Id, @TailorProfile2Id, 4250.00, 1, 1, 2, DATEADD(hour, -1, GETUTCDATE()));

-- Payment for Order 3 (Deposit)
INSERT INTO Payment (PaymentId, OrderId, CustomerId, TailorId, Amount, PaymentType, PaymentStatus, TransactionType, PaidAt)
VALUES 
    (NEWID(), @Order3Id, @CustomerProfile3Id, @TailorProfile1Id, 1400.00, 0, 1, 2, DATEADD(day, -2, GETUTCDATE()));

-- Payment for Order 4 (Deposit)
INSERT INTO Payment (PaymentId, OrderId, CustomerId, TailorId, Amount, PaymentType, PaymentStatus, TransactionType, PaidAt)
VALUES 
    (NEWID(), @Order4Id, @CustomerProfile4Id, @TailorProfile3Id, 750.00, 4, 1, 2, DATEADD(day, -9, GETUTCDATE()));

-- Payment for Order 5 (Full payment - Cash)
INSERT INTO Payment (PaymentId, OrderId, CustomerId, TailorId, Amount, PaymentType, PaymentStatus, TransactionType, PaidAt)
VALUES 
    (NEWID(), @Order5Id, @CustomerProfile5Id, @TailorProfile3Id, 800.00, 3, 1, 0, DATEADD(day, -5, GETUTCDATE()));

-- Payment for Order 6 (Full payment - Card)
INSERT INTO Payment (PaymentId, OrderId, CustomerId, TailorId, Amount, PaymentType, PaymentStatus, TransactionType, PaidAt)
VALUES 
    (NEWID(), @Order6Id, @CustomerProfile1Id, @TailorProfile1Id, 300.00, 0, 1, 0, DATEADD(day, -25, GETUTCDATE()));

PRINT '   ✅ 5 payment records created';
PRINT '';

-- ========================================
-- STEP 10: Create Reviews (for completed orders)
-- ========================================
PRINT '10. Creating Reviews...';

DECLARE @Review1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Review2Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO Reviews (ReviewId, OrderId, TailorId, CustomerId, Rating, Comment, CreatedAt, IsDeleted)
VALUES 
  (@Review1Id, @Order5Id, @TailorProfile3Id, @CustomerProfile5Id, 5, 'Excellent work! The thobe fits perfectly and the quality is outstanding. Highly recommended!', DATEADD(day, -3, GETUTCDATE()), 0),
    (@Review2Id, @Order6Id, @TailorProfile1Id, @CustomerProfile1Id, 4, 'Good alterations, professional service. Delivery was on time.', DATEADD(day, -24, GETUTCDATE()), 0);

-- Add rating dimensions for detailed feedback
INSERT INTO RatingDimensions (RatingDimensionId, ReviewId, DimensionName, Score, IsDeleted)
VALUES 
    (NEWID(), @Review1Id, 'Quality', 5, 0),
    (NEWID(), @Review1Id, 'Communication', 5, 0),
    (NEWID(), @Review1Id, 'Timeliness', 5, 0),
    (NEWID(), @Review1Id, 'Pricing', 4, 0),
  (NEWID(), @Review2Id, 'Quality', 4, 0),
    (NEWID(), @Review2Id, 'Communication', 4, 0),
    (NEWID(), @Review2Id, 'Timeliness', 5, 0),
    (NEWID(), @Review2Id, 'Pricing', 4, 0);

-- Update tailor ratings (AverageRating only, TotalReviews is computed)
UPDATE TailorProfiles SET AverageRating = 4.8 WHERE Id = @TailorProfile1Id;
UPDATE TailorProfiles SET AverageRating = 4.9 WHERE Id = @TailorProfile3Id;

PRINT '   ✅ 2 reviews with detailed ratings created';
PRINT '';

-- ========================================
-- STEP 11: Create Loyalty Transactions
-- ========================================
PRINT '11. Creating Loyalty Transactions...';

DECLARE @Loyalty1Id UNIQUEIDENTIFIER = (SELECT Id FROM CustomerLoyalty WHERE CustomerId = @CustomerProfile1Id);
DECLARE @Loyalty3Id UNIQUEIDENTIFIER = (SELECT Id FROM CustomerLoyalty WHERE CustomerId = @CustomerProfile3Id);
DECLARE @Loyalty5Id UNIQUEIDENTIFIER = (SELECT Id FROM CustomerLoyalty WHERE CustomerId = @CustomerProfile5Id);

INSERT INTO LoyaltyTransactions (Id, CustomerLoyaltyId, Points, Type, Description, RelatedOrderId, CreatedAt)
VALUES 
    (NEWID(), @Loyalty1Id, 300, 'Earned', 'Points earned from order completion', @Order6Id, DATEADD(day, -25, GETUTCDATE())),
    (NEWID(), @Loyalty1Id, -50, 'Redeemed', 'Discount coupon redeemed', NULL, DATEADD(day, -20, GETUTCDATE())),
    (NEWID(), @Loyalty3Id, 100, 'Bonus', 'Tier upgrade bonus', NULL, DATEADD(day, -15, GETUTCDATE())),
    (NEWID(), @Loyalty5Id, 800, 'Earned', 'Points earned from order completion', @Order5Id, DATEADD(day, -5, GETUTCDATE())),
    (NEWID(), @Loyalty5Id, -400, 'Redeemed', 'Premium service discount', NULL, DATEADD(day, -2, GETUTCDATE()));

PRINT '   ✅ 5 loyalty transactions created';
PRINT '';

-- ========================================
-- STEP 12: Create Sample Complaints
-- ========================================
PRINT '12. Creating Sample Complaints...';

DECLARE @Complaint1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Complaint2Id UNIQUEIDENTIFIER = NEWID();

-- Complaint 1: Open (Quality issue)
INSERT INTO Complaints (Id, OrderId, CustomerId, TailorId, Subject, Description, ComplaintType, DesiredResolution, Status, Priority, CreatedAt)
VALUES 
 (@Complaint1Id, @Order3Id, @CustomerProfile3Id, @TailorProfile1Id, 'Stitching issue on suit sleeve', 'The stitching on the left sleeve is slightly uneven. Would like it to be corrected.', 'Quality', 'Rework', 'Open', 'Medium', DATEADD(day, -1, GETUTCDATE()));

-- Complaint 2: Resolved (Delay issue)
INSERT INTO Complaints (Id, OrderId, CustomerId, TailorId, Subject, Description, ComplaintType, DesiredResolution, Status, Priority, AdminResponse, ResolvedAt, CreatedAt, UpdatedAt)
VALUES 
    (@Complaint2Id, @Order4Id, @CustomerProfile4Id, @TailorProfile3Id, 'Order delayed by 2 days', 'The order was supposed to be ready yesterday but I was informed of a delay.', 'Delay', 'Apology', 'Resolved', 'Low', 'Delay was due to fabric delivery issue. Tailor provided discount as compensation.', DATEADD(hour, -2, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()), DATEADD(hour, -2, GETUTCDATE()));

PRINT '   ✅ 2 sample complaints created';
PRINT '';

-- ========================================
-- STEP 13: Create Portfolio Images for Tailors
-- ========================================
PRINT '13. Creating Portfolio Entries...';

INSERT INTO PortfolioImages (PortfolioImageId, TailorId, ImageUrl, IsBeforeAfter, IsFeatured, DisplayOrder, UploadedAt, CreatedAt, IsDeleted, EstimatedPrice)
VALUES 
    (NEWID(), @TailorProfile1Id, '/uploads/portfolio/suit1.jpg', 0, 1, 1, GETUTCDATE(), GETUTCDATE(), 0, 2500.00),
    (NEWID(), @TailorProfile1Id, '/uploads/portfolio/thobe1.jpg', 0, 0, 2, GETUTCDATE(), GETUTCDATE(), 0, 800.00),
    (NEWID(), @TailorProfile2Id, '/uploads/portfolio/wedding1.jpg', 0, 1, 1, GETUTCDATE(), GETUTCDATE(), 0, 8000.00),
    (NEWID(), @TailorProfile2Id, '/uploads/portfolio/evening1.jpg', 0, 0, 2, GETUTCDATE(), GETUTCDATE(), 0, 3500.00),
  (NEWID(), @TailorProfile3Id, '/uploads/portfolio/abaya1.jpg', 0, 1, 1, GETUTCDATE(), GETUTCDATE(), 0, 1500.00),
    (NEWID(), @TailorProfile3Id, '/uploads/portfolio/kaftan1.jpg', 0, 0, 2, GETUTCDATE(), GETUTCDATE(), 0, 1200.00);

PRINT '   ✅ 6 portfolio entries created';
PRINT '';

-- ========================================
-- STEP 14: Create Notifications
-- ========================================
PRINT '14. Creating Sample Notifications...';

INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, SentAt)
VALUES 
    (NEWID(), @Customer1Id, 'Order Status Update', 'Your order #' + CAST(@Order1Id AS NVARCHAR(36)) + ' is awaiting tailor quote', 'OrderUpdate', 0, GETUTCDATE()),
  (NEWID(), @Customer2Id, 'Deposit Payment Received', 'Your deposit payment for order has been confirmed', 'Payment', 1, DATEADD(hour, -1, GETUTCDATE())),
  (NEWID(), @Customer3Id, 'Order In Progress', 'Your suit is now being crafted!', 'OrderUpdate', 1, DATEADD(day, -3, GETUTCDATE())),
    (NEWID(), @Customer4Id, 'Order Ready for Pickup', 'Your abaya is ready! You can pick it up anytime.', 'OrderUpdate', 0, DATEADD(hour, -6, GETUTCDATE())),
    (NEWID(), @Tailor1Id, 'New Order Received', 'You have a new order request', 'NewOrder', 0, GETUTCDATE());

PRINT '   ✅ 5 notifications created';
PRINT '';

-- ========================================
-- STEP 15: Summary Statistics
-- ========================================
PRINT '';
PRINT '========================================';
PRINT 'DATABASE SEEDING COMPLETE!';
PRINT '========================================';
PRINT '';

PRINT 'Summary of Test Data Created:';
PRINT '------------------------------';
SELECT 'Users' AS Entity, COUNT(*) AS Count FROM Users WHERE Email LIKE '%@tafsilk.test'
UNION ALL
SELECT 'Customer Profiles', COUNT(*) FROM CustomerProfiles
UNION ALL
SELECT 'Tailor Profiles', COUNT(*) FROM TailorProfiles
UNION ALL
SELECT 'Tailor Services', COUNT(*) FROM TailorServices
UNION ALL
SELECT 'Customer Loyalty', COUNT(*) FROM CustomerLoyalty
UNION ALL
SELECT 'Saved Measurements', COUNT(*) FROM CustomerMeasurements
UNION ALL
SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL
SELECT 'Payments', COUNT(*) FROM Payment
UNION ALL
SELECT 'Reviews', COUNT(*) FROM Reviews
UNION ALL
SELECT 'Loyalty Transactions', COUNT(*) FROM LoyaltyTransactions
UNION ALL
SELECT 'Complaints', COUNT(*) FROM Complaints
UNION ALL
SELECT 'Portfolio Images', COUNT(*) FROM PortfolioImages
UNION ALL
SELECT 'Notifications', COUNT(*) FROM Notifications;

PRINT '';
PRINT 'Order Status Breakdown:';
PRINT '-----------------------';
SELECT 
    CASE Status
        WHEN 0 THEN 'QuotePending'
        WHEN 1 THEN 'Confirmed'
        WHEN 2 THEN 'InProgress'
        WHEN 3 THEN 'ReadyForPickup'
        WHEN 4 THEN 'Completed'
     WHEN 5 THEN 'Cancelled'
    END AS OrderStatus,
    COUNT(*) AS Count
FROM Orders
GROUP BY Status
ORDER BY Status;

PRINT '';
PRINT 'Test Credentials:';
PRINT '-----------------';
PRINT 'All test users have the same password: Test@123';
PRINT '';
PRINT 'Customers:';
PRINT '  - ahmed.hassan@tafsilk.test (Bronze tier, 250 points)';
PRINT '  - fatima.ali@tafsilk.test (Bronze tier, 150 points)';
PRINT '  - mohamed.salem@tafsilk.test (Gold tier, 500 points)';
PRINT '  - sarah.khaled@tafsilk.test (Bronze tier, 100 points)';
PRINT '  - omar.youssef@tafsilk.test (Platinum tier, 1200 points)';
PRINT '';
PRINT 'Tailors:';
PRINT '  - master.tailor@tafsilk.test (Master Ibrahim - Suits & Thobes)';
PRINT '  - wedding.specialist@tafsilk.test (Madame Laila - Wedding Dresses)';
PRINT '  - traditional.expert@tafsilk.test (Sheikh Abdullah - Traditional Garments)';
PRINT '';
PRINT '========================================';
PRINT 'Ready for Testing!';
PRINT '========================================';
PRINT '';
PRINT 'You can now:';
PRINT '1. Login with any test account';
PRINT '2. View orders in different statuses';
PRINT '3. Test loyalty points system';
PRINT '4. Review saved measurements';
PRINT '5. Check complaint workflow';
PRINT '6. Test booking with deposits';
PRINT '';
