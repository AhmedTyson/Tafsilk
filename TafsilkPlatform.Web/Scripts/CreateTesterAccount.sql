-- ================================================================
-- TESTER ACCOUNT CREATION SCRIPT
-- Run this in SQL Server Management Studio (SSMS)
-- or Azure Data Studio to create/fix the tester account
-- ================================================================

PRINT '========================================';
PRINT '🧪 TESTER ACCOUNT SETUP STARTING...';
PRINT '========================================';

-- ✅ STEP 1: Verify/Create Roles
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
DECLARE @CustomerRoleId UNIQUEIDENTIFIER;
DECLARE @TailorRoleId UNIQUEIDENTIFIER;
DECLARE @TesterId UNIQUEIDENTIFIER;

-- Check if Admin role exists
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
  SET @AdminRoleId = NEWID();
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
VALUES (@AdminRoleId, 'Admin', 'Administrator', 100, GETUTCDATE());
    PRINT '✅ Admin role created';
END
ELSE
BEGIN
  SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Admin';
    PRINT '✅ Admin role found';
END

-- Check if Customer role exists
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Customer')
BEGIN
    SET @CustomerRoleId = NEWID();
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
    VALUES (@CustomerRoleId, 'Customer', 'Customer role', 10, GETUTCDATE());
    PRINT '✅ Customer role created';
END
ELSE
BEGIN
    SELECT @CustomerRoleId = Id FROM Roles WHERE Name = 'Customer';
    PRINT '✅ Customer role found';
END

-- Check if Tailor role exists
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Tailor')
BEGIN
    SET @TailorRoleId = NEWID();
    INSERT INTO Roles (Id, Name, Description, Priority, CreatedAt)
    VALUES (@TailorRoleId, 'Tailor', 'Tailor role', 20, GETUTCDATE());
    PRINT '✅ Tailor role created';
END
ELSE
BEGIN
    SELECT @TailorRoleId = Id FROM Roles WHERE Name = 'Tailor';
    PRINT '✅ Tailor role found';
END

-- ✅ STEP 2: Create/Update Tester User
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'tester@tafsilk.local')
BEGIN
    SET @TesterId = NEWID();
    INSERT INTO Users (Id, Email, PasswordHash, RoleId, IsActive, EmailVerified, CreatedAt, UpdatedAt)
    VALUES (
    @TesterId,
        'tester@tafsilk.local',
        -- BCrypt hash for "Tester@123!"
     '$2a$11$VvqXF4Xs3D7TnGHHZN8pPeLNq7y6Xo8bJjK8W0vH9u5J7fV5nT4Aq',
        @AdminRoleId,
        1, -- IsActive
        1, -- EmailVerified
        GETUTCDATE(),
        GETUTCDATE()
    );
    PRINT '✅ Tester user created';
END
ELSE
BEGIN
    SELECT @TesterId = Id FROM Users WHERE Email = 'tester@tafsilk.local';
    PRINT '⚠️ Tester user exists - updating password and settings';
    
    UPDATE Users
    SET PasswordHash = '$2a$11$VvqXF4Xs3D7TnGHHZN8pPeLNq7y6Xo8bJjK8W0vH9u5J7fV5nT4Aq',
        RoleId = @AdminRoleId,
      IsActive = 1,
        EmailVerified = 1,
        UpdatedAt = GETUTCDATE()
    WHERE Email = 'tester@tafsilk.local';
    PRINT '✅ Tester user updated';
END

-- ✅ STEP 3: Create Customer Profile
IF NOT EXISTS (SELECT * FROM CustomerProfiles WHERE UserId = @TesterId)
BEGIN
    INSERT INTO CustomerProfiles (Id, UserId, FullName, Gender, City, Bio, CreatedAt)
    VALUES (
        NEWID(),
        @TesterId,
    'Tester Account',
        'Other',
        'Test City',
        'Test account with full platform access for testing all pages',
        GETUTCDATE()
    );
 PRINT '✅ Customer profile created';
END
ELSE
BEGIN
UPDATE CustomerProfiles
    SET FullName = 'Tester Account',
     City = 'Test City',
        Bio = 'Test account with full platform access for testing all pages'
    WHERE UserId = @TesterId;
    PRINT '✅ Customer profile updated';
END

-- ✅ STEP 4: Create/Update Tailor Profile
IF NOT EXISTS (SELECT * FROM TailorProfiles WHERE UserId = @TesterId)
BEGIN
    INSERT INTO TailorProfiles (
        Id, UserId, FullName, ShopName, Address, City, Bio, 
        Specialization, IsVerified, ExperienceYears, AverageRating, CreatedAt
    )
    VALUES (
    NEWID(),
        @TesterId,
        'Tester Tailor',
     'Test Tailor Shop',
 'Test Address, Test City',
        'Test City',
        'Test tailor profile for testing all tailor pages and features',
N'تفصيل عام',
        1, -- IsVerified = TRUE
        5, -- ExperienceYears
    4.5, -- AverageRating
        GETUTCDATE()
    );
    PRINT '✅ Tailor profile created';
END
ELSE
BEGIN
    UPDATE TailorProfiles
    SET FullName = 'Tester Tailor',
        ShopName = 'Test Tailor Shop',
        IsVerified = 1,
        ExperienceYears = 5,
        AverageRating = 4.5
    WHERE UserId = @TesterId;
    PRINT '✅ Tailor profile updated (verified)';
END

-- ✅ VERIFICATION
PRINT '';
PRINT '========================================';
PRINT '✅ TESTER ACCOUNT SETUP COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT '📧 Email: tester@tafsilk.local';
PRINT '🔑 Password: Tester@123!';
PRINT '👤 Role: Admin';
PRINT '✅ Has Customer Profile: YES';
PRINT '✅ Has Tailor Profile: YES (Verified)';
PRINT '';
PRINT '🎯 You can now:';
PRINT '  - Login at /Account/Login';
PRINT '- Access ALL customer pages';
PRINT '  - Access ALL tailor pages';
PRINT '  - Access ALL admin pages';
PRINT '';
PRINT '========================================';

-- Show account details
PRINT '';
PRINT '📊 ACCOUNT DETAILS:';
PRINT '========================================';

SELECT 
    U.Email AS [Email],
    R.Name AS [Role],
    CASE WHEN U.IsActive = 1 THEN 'Yes' ELSE 'No' END AS [Active],
    CASE WHEN U.EmailVerified = 1 THEN 'Yes' ELSE 'No' END AS [Email Verified],
    CP.FullName AS [Customer Name],
    TP.ShopName AS [Tailor Shop],
    CASE WHEN TP.IsVerified = 1 THEN 'Yes' ELSE 'No' END AS [Tailor Verified]
FROM Users U
LEFT JOIN Roles R ON U.RoleId = R.Id
LEFT JOIN CustomerProfiles CP ON U.Id = CP.UserId
LEFT JOIN TailorProfiles TP ON U.Id = TP.UserId
WHERE U.Email = 'tester@tafsilk.local';

PRINT '';
PRINT '========================================';
PRINT '✅ SETUP COMPLETE - READY TO USE!';
PRINT '========================================';
