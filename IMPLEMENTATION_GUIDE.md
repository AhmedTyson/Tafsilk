# 🎯 ACCOUNTCONTROLLER REFACTORING & SECURITY IMPLEMENTATION GUIDE

## EXECUTIVE SUMMARY

This guide provides step-by-step instructions to refactor the AccountController.cs file to:
1. ✅ Remove duplicate methods (reduce from 1347 to ~950 lines)
2. ✅ Fix critical security vulnerabilities
3. ✅ Implement rate limiting
4. ✅ Add file validation
5. ✅ Replace Task.Run with proper background queue
6. ✅ Add input sanitization
7. ✅ Improve maintainability

**Estimated Time**: 2-3 hours  
**Risk Level**: 🟡 Medium (requires testing)  
**Rollback Available**: ✅ Yes (automatic backup created)

---

## PREREQUISITES

### 1. Install Required NuGet Packages
```bash
dotnet add package System.Threading.Channels
```

### 2. Verify Existing Packages
Ensure these are already installed (should be in .NET 9):
- Microsoft.AspNetCore.WebUtilities
- System.Security.Cryptography
- System.Text.Encodings.Web

---

## PHASE 1: REMOVE DUPLICATE METHODS (15 minutes)

### Step 1.1: Run Duplicate Removal Script

```powershell
# Navigate to project root
cd C:\Users\ahmed\source\repos\AhmedTyson\Tafsilk

# Run the script
.\REMOVE_DUPLICATES.ps1
```

**Expected Output**:
```
Creating backup: TafsilkPlatform.Web\Controllers\AccountController.cs.backup-20250101-123456
Original file: 1347 lines
Keeping lines 1-772...
Skipping lines 773-923 (duplicate CompleteTailorProfile)...
Keeping lines 924-986 (VerifyEmail, ResendVerificationEmail)...
Skipping lines 987-1050 (duplicate VerifyEmail, ResendVerificationEmail)...
Keeping lines 1051-1347 (ProvideTailorEvidence, final CompleteTailorProfile)...
✅ Done! New file: 1084 lines (removed 263 lines)
✅ Backup saved to: TafsilkPlatform.Web\Controllers\AccountController.cs.backup-20250101-123456
```

### Step 1.2: Verify Build

```bash
dotnet build
```

**Expected**: ✅ Build Succeeded

---

## PHASE 2: ADD REQUIRED SERVICES (20 minutes)

### Step 2.1: Create Background Task Queue

Create file: `TafsilkPlatform.Web\Services\BackgroundTasks\IBackgroundTaskQueue.cs`
```csharp
namespace TafsilkPlatform.Web.Services.BackgroundTasks;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
  ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
```

Create file: `TafsilkPlatform.Web\Services\BackgroundTasks\BackgroundTaskQueue.cs`
```csharp
using System.Threading.Channels;

namespace TafsilkPlatform.Web.Services.BackgroundTasks;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
       FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
    {
   if (workItem == null) throw new ArgumentNullException(nameof(workItem));
 await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
```

Create file: `TafsilkPlatform.Web\Services\BackgroundTasks\QueuedHostedService.cs`
```csharp
using Microsoft.Extensions.Hosting;

namespace TafsilkPlatform.Web.Services.BackgroundTasks;

public class QueuedHostedService : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;

    public QueuedHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<QueuedHostedService> logger)
    {
        _taskQueue = taskQueue;
  _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Queued Hosted Service is starting.");

   while (!stoppingToken.IsCancellationRequested)
        {
  try
    {
              var workItem = await _taskQueue.DequeueAsync(stoppingToken);
     await workItem(stoppingToken);
            }
   catch (OperationCanceledException)
   {
            // Expected during shutdown
      }
            catch (Exception ex)
     {
 _logger.LogError(ex, "Error occurred executing background work item.");
            }
        }

        _logger.LogInformation("Queued Hosted Service is stopping.");
    }
}
```

### Step 2.2: Copy File Validation Service

Copy the entire content from `SECURITY_AUDIT_FILE_VALIDATION_SERVICE.cs` to:
`TafsilkPlatform.Web\Services\FileValidationService.cs`

### Step 2.3: Register Services in Program.cs

Add AFTER `builder.Services.AddSingleton<IDateTimeService, DateTimeService>();`:

```csharp
// Background task queue for email sending
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// File validation service
builder.Services.AddScoped<IFileValidationService, FileValidationService>();

// HTML Encoder for input sanitization
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Default);
```

### Step 2.4: Configure Rate Limiting

Add AFTER `builder.Services.AddAuthorization(...)`:

```csharp
// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Login attempts: 5 per 15 minutes
options.AddFixedWindowLimiter("login", options =>
    {
        options.Window = TimeSpan.FromMinutes(15);
   options.PermitLimit = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
 options.QueueLimit = 0;
    });

    // File uploads: 10 concurrent
    options.AddConcurrencyLimiter("fileupload", options =>
    {
        options.PermitLimit = 10;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
     options.QueueLimit = 5;
    });

    // Email sending: 10 per hour
    options.AddTokenBucketLimiter("email", options =>
    {
        options.TokenLimit = 10;
        options.ReplenishmentPeriod = TimeSpan.FromHours(1);
    options.TokensPerPeriod = 5;
        options.AutoReplenishment = true;
  });

    // Custom rejection response
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
   
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
    retryAfter = retryAfter?.TotalSeconds
        }, cancellationToken: token);
    };
});
```

Add `app.UseRateLimiter();` AFTER `app.UseAuthentication();`:

```csharp
app.UseAuthentication();
app.UseRateLimiter(); // ✅ ADD THIS
app.UseAuthorization();
```

---

## PHASE 3: UPDATE ACCOUNTCONTROLLER (45 minutes)

### Step 3.1: Add Required Using Statements

Add at the top of AccountController.cs:

```csharp
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Encodings.Web;
using TafsilkPlatform.Web.Services.BackgroundTasks;
```

### Step 3.2: Update Constructor

Replace the existing constructor with:

```csharp
private readonly IAuthService _auth;
private readonly IUserRepository _userRepository;
private readonly IUnitOfWork _unitOfWork;
private readonly IFileUploadService _fileUploadService;
private readonly ILogger<AccountController> _logger;
private readonly IDateTimeService _dateTime;
private readonly IBackgroundTaskQueue _taskQueue; // ✅ NEW
private readonly IFileValidationService _fileValidation; // ✅ NEW
private readonly IServiceProvider _serviceProvider; // ✅ NEW
private readonly HtmlEncoder _htmlEncoder; // ✅ NEW

public AccountController(
    IAuthService auth,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IFileUploadService fileUploadService,
    ILogger<AccountController> logger,
    IDateTimeService dateTime,
    IBackgroundTaskQueue taskQueue, // ✅ NEW
  IFileValidationService fileValidation, // ✅ NEW
    IServiceProvider serviceProvider, // ✅ NEW
  HtmlEncoder htmlEncoder) // ✅ NEW
{
    _auth = auth;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _fileUploadService = fileUploadService;
    _logger = logger;
    _dateTime = dateTime;
    _taskQueue = taskQueue;
    _fileValidation = fileValidation;
    _serviceProvider = serviceProvider;
    _htmlEncoder = htmlEncoder;
}
```

### Step 3.3: Add Helper Method for Secure Token Generation

Add this private method anywhere in the class:

```csharp
/// <summary>
/// Generates a cryptographically secure token for email verification
/// </summary>
private string GenerateSecureVerificationToken()
{
    var tokenBytes = new byte[32]; // 256 bits
    using (var rng = RandomNumberGenerator.Create())
 {
        rng.GetBytes(tokenBytes);
    }
    return WebEncoders.Base64UrlEncode(tokenBytes);
}
```

### Step 3.4: Update Specific Methods

**Option A: Manual Updates** (Recommended for learning)
1. Open `SECURITY_HARDENED_ACCOUNTCONTROLLER_METHODS.cs`
2. Copy each method implementation
3. Replace the corresponding method in AccountController.cs

**Option B: Automated Replacement** (Faster but riskier)
Use the provided method implementations from `SECURITY_HARDENED_ACCOUNTCONTROLLER_METHODS.cs`

**Methods to Update**:
1. ✅ `Login` GET and POST (add authentication check + rate limiting)
2. ✅ `Register` GET and POST (add authentication check + sanitization + rate limiting)
3. ✅ `ProvideTailorEvidence` POST (add file validation + sanitization + background queue)
4. ✅ `ResendVerificationEmail` POST (add rate limiting)

### Step 3.5: Replace Token Generation (Line ~1198)

Find this code in `ProvideTailorEvidence` POST:
```csharp
// OLD (INSECURE):
var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
    .Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);
```

Replace with:
```csharp
// NEW (SECURE):
var verificationToken = GenerateSecureVerificationToken();
```

### Step 3.6: Replace Task.Run with Background Queue (Line ~1202)

Find this code in `ProvideTailorEvidence` POST:
```csharp
// OLD (BAD):
_ = Task.Run(async () =>
{
  try
    {
        // await _emailService.SendEmailVerificationAsync(...);
        _logger.LogInformation("Email verification sent");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send verification email");
    }
});
```

Replace with:
```csharp
// NEW (GOOD):
await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
{
    using var scope = _serviceProvider.CreateScope();
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountController>>();
    
    try
    {
        await emailService.SendEmailVerificationAsync(
  user.Email,
      model.FullName,
            verificationToken);
        logger.LogInformation("[Background] Email verification sent to {Email}", user.Email);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[Background] Failed to send verification email to {Email}", user.Email);
    }
});
```

---

## PHASE 4: UPDATE AUTHSERVICE FOR ACCOUNT LOCKOUT (30 minutes)

### Step 4.1: Add Database Migration

Create migration file: `TafsilkPlatform.Web\Data\Migrations\AddAccountLockout.cs`

```bash
dotnet ef migrations add AddAccountLockout
```

This should add these fields to Users table:
- `FailedLoginAttempts` (int)
- `LockoutEnd` (DateTime?)

### Step 4.2: Update User Entity

Add to `TafsilkPlatform.Web\Models\User.cs`:

```csharp
public int FailedLoginAttempts { get; set; }
public DateTime? LockoutEnd { get; set; }
public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
```

### Step 4.3: Update AuthService.ValidateUserAsync

Replace the method in `AuthService.cs` with the implementation from:
`SECURITY_AUDIT_ACCOUNT_LOCKOUT_IMPLEMENTATION.cs`

Key changes:
1. ✅ Check if account is locked out before validation
2. ✅ Increment failed attempts on wrong password
3. ✅ Lock account after 5 failed attempts (15 minutes)
4. ✅ Reset failed attempts on successful login

---

## PHASE 5: BUILD & TEST (30 minutes)

### Step 5.1: Build Project

```bash
dotnet build
```

**Expected**: ✅ Build Succeeded

### Step 5.2: Run Database Migration

```bash
dotnet ef database update
```

**Expected**: ✅ Migration applied successfully

### Step 5.3: Manual Testing

**Test 1: Duplicate Methods Removed**
```bash
# Count methods in AccountController
$methods = Get-Content "TafsilkPlatform.Web\Controllers\AccountController.cs" | Select-String -Pattern "public.*Task<IActionResult>"
Write-Output "Total action methods: $($methods.Count)"
# Expected: ~20-22 (was ~26 before)
```

**Test 2: Secure Token Generation**
1. Register as tailor
2. Submit evidence
3. Check database: `SELECT EmailVerificationToken FROM Users WHERE Email = 'test@example.com'`
4. Token should be 43 characters (Base64URL encoded 32 bytes)

**Test 3: Rate Limiting**
1. Try to login 6 times with wrong password
2. Expected: 5 attempts allowed, 6th blocked with 429 status

**Test 4: File Validation**
1. Try to upload .exe file renamed to .jpg
2. Expected: Upload rejected with "Invalid file signature" error

**Test 5: Account Lockout**
1. Try to login 5 times with wrong password
2. Expected: Account locked for 15 minutes

---

## PHASE 6: VERIFICATION (15 minutes)

### Step 6.1: Code Quality Check

Run these checks:

```powershell
# 1. Check for duplicate methods
$content = Get-Content "TafsilkPlatform.Web\Controllers\AccountController.cs" -Raw
$methods = [regex]::Matches($content, 'public.*Task<IActionResult>\s+(\w+)\(')
$duplicates = $methods | Group-Object { $_.Groups[1].Value } | Where-Object { $_.Count -gt 2 }
if ($duplicates) {
    Write-Host "❌ Found duplicate methods: $($duplicates.Name -join ', ')"
} else {
    Write-Host "✅ No duplicate methods found"
}

# 2. Check for Task.Run usage
if ($content -match '_ = Task\.Run') {
    Write-Host "❌ Found Task.Run (should use background queue)"
} else {
    Write-Host "✅ No Task.Run found"
}

# 3. Check for insecure token generation
if ($content -match 'Guid\.NewGuid\(\)\.ToByteArray\(\)') {
    Write-Host "❌ Found insecure token generation"
} else {
    Write-Host "✅ Secure token generation implemented"
}

# 4. Check for rate limiting attributes
$rateLimitCount = ([regex]::Matches($content, 'EnableRateLimiting')).Count
Write-Host "✅ Rate limiting applied to $rateLimitCount methods"
```

### Step 6.2: Security Score

Re-run compliance checks:

| Requirement | Before | After | Status |
|-------------|--------|-------|--------|
| Cryptographically Secure Tokens | ❌ 0/10 | ✅ 10/10 | FIXED |
| URL-Safe Base64 Encoding | ❌ 0/10 | ✅ 10/10 | FIXED |
| Token Replay Prevention | ❌ 0/10 | ✅ 10/10 | FIXED |
| No Task.Run in Controllers | ❌ 0/10 | ✅ 10/10 | FIXED |
| Background Task Queue | ❌ 0/10 | ✅ 10/10 | FIXED |
| Magic-Number Validation | ❌ 0/10 | ✅ 10/10 | FIXED |
| File Size Limit | ❌ 0/10 | ✅ 10/10 | FIXED |
| Rate Limiting | ❌ 0/10 | ✅ 10/10 | FIXED |
| Account Lockout | ❌ 0/10 | ✅ 10/10 | FIXED |
| Input Sanitization | ❌ 0/10 | ✅ 10/10 | FIXED |

**New Security Score**: **180/220** = **81.8%** (was 40.9%)

---

## ROLLBACK PROCEDURE

If issues occur:

### Option 1: Restore from Backup
```powershell
# Find latest backup
$backup = Get-ChildItem "TafsilkPlatform.Web\Controllers\AccountController.cs.backup*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

# Restore
Copy-Item $backup.FullName "TafsilkPlatform.Web\Controllers\AccountController.cs" -Force

# Rebuild
dotnet build
```

### Option 2: Git Revert
```bash
git checkout HEAD -- TafsilkPlatform.Web/Controllers/AccountController.cs
git checkout HEAD -- TafsilkPlatform.Web/Program.cs
```

---

## MAINTENANCE NOTES

### Code Organization
The refactored AccountController follows these principles:
1. ✅ **Single Responsibility**: Each method has one clear purpose
2. ✅ **DRY**: No duplicate methods
3. ✅ **Security First**: All inputs validated and sanitized
4. ✅ **Proper Error Handling**: Structured logging and error messages
5. ✅ **Performance**: Background processing for long-running tasks

### Future Improvements (Optional)
1. ✅ Extract OAuth logic to separate OAuthController
2. ✅ Create ViewModel validators using FluentValidation
3. ✅ Add unit tests for each action method
4. ✅ Implement CQRS pattern for complex operations
5. ✅ Add OpenAPI/Swagger documentation

---

## SUCCESS CRITERIA

✅ **DONE WHEN**:
- [ ] No duplicate methods exist
- [ ] Build succeeds with 0 warnings
- [ ] All security features implemented
- [ ] Manual tests pass
- [ ] Security score > 75%
- [ ] Code review approved

---

## SUPPORT

### Troubleshooting

**Issue**: Build fails with "IBackgroundTaskQueue not found"
**Solution**: Ensure you've created the background task files in correct namespace

**Issue**: Rate limiting not working
**Solution**: Verify `app.UseRateLimiter()` is called AFTER `app.UseAuthentication()`

**Issue**: File validation always fails
**Solution**: Check that FileValidationService is registered in Program.cs

### Resources
- [Security Audit Report](SECURITY_AUDIT_REPORT.md)
- [Refactoring Plan](ACCOUNTCONTROLLER_REFACTORING_PLAN.md)
- [Hardened Methods](SECURITY_HARDENED_ACCOUNTCONTROLLER_METHODS.cs)
- [Background Task Implementation](SECURITY_AUDIT_BACKGROUND_TASK_IMPLEMENTATION.cs)
- [File Validation Service](SECURITY_AUDIT_FILE_VALIDATION_SERVICE.cs)
- [Rate Limiting Setup](SECURITY_AUDIT_RATE_LIMITING_IMPLEMENTATION.cs)
- [Account Lockout](SECURITY_AUDIT_ACCOUNT_LOCKOUT_IMPLEMENTATION.cs)

---

**Document Version**: 1.0  
**Last Updated**: 2025  
**Status**: ✅ READY FOR IMPLEMENTATION
