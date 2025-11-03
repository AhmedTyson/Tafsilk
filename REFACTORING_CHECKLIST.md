# ✅ ACCOUNTCONTROLLER REFACTORING CHECKLIST

## QUICK REFERENCE - Execute in Order

### ☐ PHASE 1: BACKUP & PREPARATION (5 min)
```powershell
# 1. Create backup
cd C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk
Copy-Item "TafsilkPlatform.Web\Controllers\AccountController.cs" "TafsilkPlatform.Web\Controllers\AccountController.cs.BACKUP-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# 2. Commit current state to Git
git add .
git commit -m "Before AccountController refactoring"
```

### ☐ PHASE 2: REMOVE DUPLICATES (10 min)
```powershell
# Run duplicate removal script
.\REMOVE_DUPLICATES.ps1

# Verify build
dotnet build
```
**Expected**: Build succeeds, ~263 lines removed

### ☐ PHASE 3: ADD SERVICES (20 min)

#### Create Background Task Queue Files
- [ ] Create `Services\BackgroundTasks\IBackgroundTaskQueue.cs`
- [ ] Create `Services\BackgroundTasks\BackgroundTaskQueue.cs`
- [ ] Create `Services\BackgroundTasks\QueuedHostedService.cs`

#### Create File Validation Service
- [ ] Copy `SECURITY_AUDIT_FILE_VALIDATION_SERVICE.cs` content
- [ ] Create `Services\FileValidationService.cs`
- [ ] Create `Interfaces\IFileValidationService.cs`

#### Update Program.cs
```csharp
// Add after IDateTimeService registration:
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Default);

// Add rate limiting configuration (see IMPLEMENTATION_GUIDE.md)
builder.Services.AddRateLimiter(options => { ... });

// Add middleware (after UseAuthentication):
app.UseRateLimiter();
```

### ☐ PHASE 4: UPDATE ACCOUNTCONTROLLER (45 min)

#### Add Using Statements
```csharp
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Encodings.Web;
using TafsilkPlatform.Web.Services.BackgroundTasks;
```

#### Update Constructor
- [ ] Add `IBackgroundTaskQueue _taskQueue`
- [ ] Add `IFileValidationService _fileValidation`
- [ ] Add `IServiceProvider _serviceProvider`
- [ ] Add `HtmlEncoder _htmlEncoder`

#### Add Helper Method
- [ ] Add `GenerateSecureVerificationToken()` method

#### Update Methods
- [ ] `Login` GET: Add authentication check
- [ ] `Login` POST: Add `[EnableRateLimiting("login")]` + auth check
- [ ] `Register` GET: Add authentication check
- [ ] `Register` POST: Add `[EnableRateLimiting("login")]` + sanitization
- [ ] `ProvideTailorEvidence` POST: Add file validation + background queue + `[EnableRateLimiting("fileupload")]`
- [ ] `ResendVerificationEmail` POST: Add `[EnableRateLimiting("email")]`

#### Fix Token Generation
Replace:
```csharp
// OLD:
var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())...

// NEW:
var verificationToken = GenerateSecureVerificationToken();
```

#### Fix Background Email Sending
Replace:
```csharp
// OLD:
_ = Task.Run(async () => { ... });

// NEW:
await _taskQueue.QueueBackgroundWorkItemAsync(async token => { ... });
```

### ☐ PHASE 5: UPDATE AUTHSERVICE (30 min)

#### Database Migration
```bash
dotnet ef migrations add AddAccountLockout
dotnet ef database update
```

#### Update User Entity
- [ ] Add `FailedLoginAttempts` property
- [ ] Add `LockoutEnd` property
- [ ] Add `IsLockedOut` computed property

#### Update ValidateUserAsync
- [ ] Add lockout check
- [ ] Add failed attempt increment
- [ ] Add lockout after 5 failures
- [ ] Add reset on successful login

### ☐ PHASE 6: BUILD & TEST (30 min)

#### Build
```bash
dotnet build
```
- [ ] ✅ Build succeeded
- [ ] ✅ 0 errors
- [ ] ✅ 0 warnings

#### Unit Tests
- [ ] Test secure token generation (43 characters)
- [ ] Test rate limiting (6th login blocked)
- [ ] Test file validation (reject .exe as .jpg)
- [ ] Test account lockout (5 failures = locked)
- [ ] Test background queue (email sent async)

#### Integration Tests
- [ ] Complete tailor registration flow
- [ ] ONE-TIME evidence submission
- [ ] Cannot submit evidence twice
- [ ] Cannot access Login when authenticated
- [ ] Cannot access Register when authenticated

### ☐ PHASE 7: VERIFICATION (15 min)

#### Code Quality
```powershell
# Check for duplicates
$content = Get-Content "TafsilkPlatform.Web\Controllers\AccountController.cs" -Raw
$methods = [regex]::Matches($content, 'public.*Task<IActionResult>\s+(\w+)\(')
$duplicates = $methods | Group-Object { $_.Groups[1].Value } | Where-Object { $_.Count -gt 2 }
Write-Host "Duplicate methods: $($duplicates.Count) (should be 0)"

# Check for Task.Run
if ($content -match '_ = Task\.Run') { Write-Host "❌ Still has Task.Run" } else { Write-Host "✅ No Task.Run" }

# Check for insecure token
if ($content -match 'Guid\.NewGuid\(\)\.ToByteArray\(\)') { Write-Host "❌ Insecure token" } else { Write-Host "✅ Secure token" }

# Count rate limiting
$rateLimitCount = ([regex]::Matches($content, 'EnableRateLimiting')).Count
Write-Host "Rate limiting attributes: $rateLimitCount (should be 4+)"
```

#### Security Score
Run manual security checklist:
- [ ] Cryptographically secure tokens: ✅
- [ ] URL-safe Base64 encoding: ✅
- [ ] Token replay prevention: ✅
- [ ] No Task.Run in controllers: ✅
- [ ] Background task queue: ✅
- [ ] Magic-number validation: ✅
- [ ] File size limit: ✅
- [ ] Rate limiting: ✅
- [ ] Account lockout: ✅
- [ ] Input sanitization: ✅

**Target Score**: > 75% (was 40.9%)

---

## QUICK VERIFICATION COMMANDS

```powershell
# File size check
$lines = (Get-Content "TafsilkPlatform.Web\Controllers\AccountController.cs").Count
Write-Host "AccountController lines: $lines (target: ~950, was: 1347)"

# Build check
dotnet build --no-restore

# Migration check
dotnet ef migrations list

# Service registration check
Get-Content "TafsilkPlatform.Web\Program.cs" | Select-String -Pattern "BackgroundTaskQueue|FileValidationService|RateLimiter"
```

---

## ROLLBACK IF NEEDED

```powershell
# Option 1: Restore from backup
$backup = Get-ChildItem "TafsilkPlatform.Web\Controllers\AccountController.cs.BACKUP*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Copy-Item $backup.FullName "TafsilkPlatform.Web\Controllers\AccountController.cs" -Force

# Option 2: Git revert
git checkout HEAD -- TafsilkPlatform.Web/Controllers/AccountController.cs
git checkout HEAD -- TafsilkPlatform.Web/Program.cs

# Rebuild
dotnet build
```

---

## SUCCESS CRITERIA

✅ **COMPLETE WHEN ALL TRUE**:
- [ ] No duplicate methods
- [ ] Build succeeds (0 errors, 0 warnings)
- [ ] All rate limiting attributes added
- [ ] File validation implemented
- [ ] Background queue replaces Task.Run
- [ ] Secure token generation
- [ ] Account lockout working
- [ ] Input sanitization applied
- [ ] Manual tests pass
- [ ] Security score > 75%

---

## TIME ESTIMATE

| Phase | Time | Status |
|-------|------|--------|
| Backup & Preparation | 5 min | ☐ |
| Remove Duplicates | 10 min | ☐ |
| Add Services | 20 min | ☐ |
| Update AccountController | 45 min | ☐ |
| Update AuthService | 30 min | ☐ |
| Build & Test | 30 min | ☐ |
| Verification | 15 min | ☐ |
| **TOTAL** | **~2.5 hours** | ☐ |

---

## NOTES

- Keep IMPLEMENTATION_GUIDE.md open for detailed instructions
- Test after each phase
- Don't skip verification steps
- Commit after each successful phase

---

**Status**: 📋 READY TO EXECUTE  
**Risk**: 🟡 Medium  
**Rollback**: ✅ Available  
**Support**: See IMPLEMENTATION_GUIDE.md
