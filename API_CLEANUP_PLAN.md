# Unnecessary APIs and Features Removal Plan

## Analysis Summary

After analyzing the codebase, the following APIs and features are identified as UNUSED or UNNECESSARY:

### 1. ✅ Refresh Token System (COMPLETELY UNUSED)
**Status**: Not implemented, just placeholder
- `RefreshToken` model exists but never created
- `RefreshToken` endpoint returns "not supported" message
- No actual refresh logic anywhere
- Just taking up database space

**Files to Remove**:
- `Models\RefreshToken.cs`
- RefreshToken DbSet from AppDbContext
- RefreshToken navigation from User model
- RefreshToken configuration in AppDbContext

### 2. ✅ Duplicate Auth API Controllers
**Status**: TWO controllers doing the same thing
- `ApiAuthController.cs` - Full JWT authentication with registration
- `AuthApiController.cs` - Minimal token creation only
- Confusing and redundant

**Action**: Keep `ApiAuthController.cs` (more complete), remove `AuthApiController.cs`

### 3. ⚠️ OrdersApiController (KEEP)
**Status**: Actually used for idempotent order creation
- Has idempotency support
- Background cleanup service
- **KEEP THIS** - it's functional

### 4. ✅ Email Verification Tokens (PARTIALLY UNUSED)
**Status**: Fields exist but no email sending implemented
- `EmailVerificationToken` in User model
- No actual email verification flow
- Can keep fields but not critical

**Action**: Keep for future, but document as not implemented

### 5. ✅ Notification Preferences (UNUSED after notification removal)
**Status**: Fields exist but notifications removed
- `EmailNotifications`, `SmsNotifications`, `PromotionalNotifications`
- Notifications system already removed

**Action**: Remove these fields from User model

## Removal Priority

### HIGH PRIORITY (Remove Now)
1. RefreshToken model and database table
2. Duplicate AuthApiController
3. Notification preference fields from User
4. RefreshToken navigation from User

### MEDIUM PRIORITY (Consider)
1. Email verification tokens (if not planning to implement)
2. Password reset tokens (if using different method)

### KEEP
1. ApiAuthController (main JWT auth)
2. OrdersApiController (idempotent orders)
3. Ban management fields (useful)

## Database Impact

Tables to drop:
- `RefreshTokens` table

Fields to remove from Users table:
- `EmailNotifications`
- `SmsNotifications`
- `PromotionalNotifications`
