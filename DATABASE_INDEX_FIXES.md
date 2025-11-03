# Database Performance Index Fixes

## Issue Summary
The performance indexes were failing to apply due to incorrect column names that didn't match the actual database schema.

## Errors Fixed

### 1. Orders Table - TotalAmount → TotalPrice
**Error:** `Column name 'TotalAmount' does not exist in the target table or view.`

**Fixed Indexes:**
- `IX_Orders_CustomerId_Status` - Now uses `TotalPrice` in INCLUDE clause
- `IX_Orders_TailorId_Status` - Now uses `TotalPrice` in INCLUDE clause

### 2. RefreshTokens Table - IsRevoked → RevokedAt
**Error:** `Invalid column name 'IsRevoked'.`

**Fixed Index:**
- `IX_RefreshTokens_UserId_ExpiresAt` - Now uses `WHERE [RevokedAt] IS NULL` instead of `WHERE [IsRevoked] = 0`

## Files Modified

### 1. TafsilkPlatform.Web\Extensions\DatabaseInitializationExtensions.cs
- Updated Index 5 (Customer Orders) to use `TotalPrice` instead of `TotalAmount`
- Updated Index 6 (Tailor Orders) to use `TotalPrice` instead of `TotalAmount`
- Updated Index 9 (Refresh Tokens) to use `WHERE [RevokedAt] IS NULL` instead of `WHERE [IsRevoked] = 0`

### 2. TafsilkPlatform.Web\Scripts\01_AddPerformanceIndexes.sql
- Already contains the fixes (comments show FIXED)
- The SQL script file is correct

## Verification

✅ **Build Status:** Successful  
✅ **Compilation:** No errors  
✅ **Schema Match:** All indexes now reference correct column names  

## Next Steps

1. **Restart the application** to apply the corrected indexes
2. **Verify indexes are created** by checking the application logs
3. **Confirm no errors** in the database initialization process

## Expected Log Output

You should now see:
```
info: Program[0]
      ✓ Applied 10 performance indexes
```

Instead of:
```
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand...
      Column name 'TotalAmount' does not exist...
```

## Database Schema Reference

**Orders Table:**
- Uses `TotalPrice` (decimal) - NOT TotalAmount

**RefreshTokens Table:**
- Uses `RevokedAt` (DateTime?) - NOT IsRevoked (bool)
- Active tokens have `RevokedAt IS NULL`
- Revoked tokens have a timestamp in `RevokedAt`
