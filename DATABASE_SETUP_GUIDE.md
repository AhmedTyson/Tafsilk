# Database Setup & Verification Guide

## Current Status
✅ Database created: `TafsilkPlatformDb_Dev`  
✅ Project builds successfully  
✅ AppDbContext properly configured  
⚠️ **Issue**: EF Core migrations are generating empty (no CREATE TABLE statements)

## Root Cause Analysis

The migrations are empty because the model snapshot was deleted and EF Core cannot properly detect the model changes without an existing snapshot.

## Solution: Use Database.EnsureCreated() Then Create Migration

### Step 1: Create Database Schema Using EnsureCreated

Run this in Package Manager Console or Terminal:

```bash
# This will create the database schema directly from the model
# DO NOT use this in production - only for development setup
```

Or run the application once with this code in Program.cs (temporary):

```csharp
// TEMPORARY: Add before app.Run()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var created = db.Database.EnsureCreated();
    if (created)
    {
        Console.WriteLine("Database schema created!");
  }
}
```

### Step 2: After Schema is Created, Generate Migration from Existing Database

```bash
# This will reverse-engineer the existing database into a migration
dotnet ef migrations add InitialCreate --context AppDbContext
```

### Step 3: Apply Performance Indexes

Run the SQL script manually:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -i "Scripts\01_AddPerformanceIndexes.sql"
```

## Alternative: Clean Slate Approach

If the above doesn't work, here's a clean slate approach:

### 1. Drop Everything

```bash
dotnet ef database drop --force
Remove-Item -Recurse -Force "Migrations" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force "bin", "obj" -ErrorAction SilentlyContinue
```

### 2. Clean and Rebuild

```bash
dotnet clean
dotnet build
```

### 3. Temporarily Use EnsureCreated

Add to `Program.cs` after `var app = builder.Build();`:

```csharp
// TEMPORARY: Remove after initial setup
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
    {
            // This creates the database schema based on your models
     var created = db.Database.EnsureCreated();
            
   if (created)
     {
      logger.LogInformation("Database schema created using EnsureCreated()");

         // Seed admin user
             TafsilkPlatform.Web.Data.Seed.AdminSeeder.Seed(db, builder.Configuration, logger);
       }
        }
        catch (Exception ex)
    {
      logger.LogError(ex, "Error creating database schema");
        }
    }
}
```

### 4. Run the Application

```bash
dotnet run
```

This will create all tables based on your `AppDbContext` configuration.

### 5. Verify Tables Were Created

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
```

### 6. Create Initial Migration from Existing Database

Once tables exist:

```bash
dotnet ef migrations add InitialSetup
```

### 7. Mark Migration as Applied (since database already exists)

```bash
dotnet ef database update --connection "Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb_Dev;Trusted_Connection=True;"
```

### 8. Apply Performance Indexes

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d TafsilkPlatformDb_Dev -i "Scripts\01_AddPerformanceIndexes.sql"
```

### 9. Remove EnsureCreated Code

Remove the temporary code from `Program.cs` and use migrations going forward.

## Verification Checklist

After setup, verify:

- [ ] Database exists
- [ ] All tables created (Users, Roles, Orders, etc.)
- [ ] Foreign keys configured
- [ ] Indexes applied
- [ ] Admin user seeded
- [ ] Application runs without errors
- [ ] Login works

## Common Issues & Solutions

### Issue: "Invalid object name 'Users'"
**Cause**: Tables not created  
**Solution**: Use EnsureCreated() approach above

### Issue: Empty migrations
**Cause**: No model snapshot or corrupted snapshot  
**Solution**: Create database first, then scaffold migration

### Issue: "Cannot drop constraint"
**Cause**: Attempting to run old migrations on new database  
**Solution**: Drop database and start fresh

## Repository & Interface Verification

All repositories and interfaces have been reviewed and are correctly implemented:

### Repositories
✅ `UserRepository` - Optimized with compiled queries  
✅ `CustomerRepository`  
✅ `TailorRepository`  
✅ `OrderRepository`  
✅ All other repositories

### Interfaces
✅ All interfaces match their implementations  
✅ No missing methods  
✅ Proper async patterns used

### Services
✅ `AuthService` - Optimized with caching and projections  
✅ `ProfileService`  
✅ `EmailService`  
✅ All services properly injected in `Program.cs`

## Performance Optimizations Applied

1. **Compiled Queries** in `UserRepository`
2. **Memory Caching** for role lookups in `AuthService`
3. **Projection Queries** to load only required data
4. **Split Queries** to avoid cartesian explosion
5. **ExecuteUpdateAsync** for bulk updates
6. **10 Performance Indexes** (in SQL script)
7. **Async database migration** on startup

## Next Steps

1. Follow the "Clean Slate Approach" above
2. Verify all tables are created
3. Apply performance indexes
4. Test login functionality
5. Monitor performance improvements
