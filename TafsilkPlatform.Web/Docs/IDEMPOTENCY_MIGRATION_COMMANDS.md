# Database Migration Commands for Idempotency Keys

## Create Migration
```powershell
cd TafsilkPlatform.Web
dotnet ef migrations add AddIdempotencyKeysTable --context AppDbContext
```

## Apply Migration to Database
```powershell
dotnet ef database update --context AppDbContext
```

## Verify Migration
```sql
-- Check if table was created
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IdempotencyKeys'

-- Check table structure
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'IdempotencyKeys'

-- View data
SELECT * FROM IdempotencyKeys ORDER BY CreatedAtUtc DESC
```

## Rollback Migration (if needed)
```powershell
dotnet ef database update <PreviousMigrationName> --context AppDbContext
dotnet ef migrations remove --context AppDbContext
```

## Test Queries
```sql
-- View active idempotency keys
SELECT Key, Status, CreatedAtUtc, ExpiresAtUtc, Endpoint, Method
FROM IdempotencyKeys
WHERE Status = 1 -- Completed
ORDER BY CreatedAtUtc DESC

-- Count by status
SELECT Status, COUNT(*) as Count
FROM IdempotencyKeys
GROUP BY Status

-- Find expired keys
SELECT COUNT(*) as ExpiredCount
FROM IdempotencyKeys
WHERE ExpiresAtUtc < GETUTCDATE()

-- Manual cleanup (if needed)
DELETE FROM IdempotencyKeys WHERE ExpiresAtUtc < GETUTCDATE()
```
