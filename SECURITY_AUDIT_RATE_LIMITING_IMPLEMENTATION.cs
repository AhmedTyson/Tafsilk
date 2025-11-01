// RATE LIMITING IMPLEMENTATION FOR .NET 9

// In Program.cs, add:
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

// Add rate limiting services
builder.Services.AddRateLimiter(options =>
{
    // Fixed window rate limiter for login attempts
    options.AddFixedWindowLimiter("login", options =>
    {
        options.Window = TimeSpan.FromMinutes(15);
 options.PermitLimit = 5; // 5 attempts per 15 minutes
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0; // No queueing
    });

    // Sliding window for general API endpoints
    options.AddSlidingWindowLimiter("api", options =>
    {
    options.Window = TimeSpan.FromMinutes(1);
        options.PermitLimit = 60;
        options.SegmentsPerWindow = 6;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    // Concurrency limiter for file uploads
  options.AddConcurrencyLimiter("fileupload", options =>
    {
        options.PermitLimit = 10;
      options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });

    // Token bucket for email sending
    options.AddTokenBucketLimiter("email", options =>
    {
 options.TokenLimit = 10;
        options.ReplenishmentPeriod = TimeSpan.FromHours(1);
        options.TokensPerPeriod = 5;
        options.AutoReplenishment = true;
    });

    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
      var userIdentifier = context.User.Identity?.Name ?? context.Request.Headers["X-Forwarded-For"].ToString() ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
         partitionKey: userIdentifier,
            factory: partition => new FixedWindowRateLimiterOptions
   {
                Window = TimeSpan.FromMinutes(1),
         PermitLimit = 100,
   QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
         });
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

// In middleware pipeline (after UseRouting):
app.UseRateLimiter();

// In AccountController - Apply to Login action:
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
[EnableRateLimiting("login")] // ✅ ADD THIS
public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
{
    // ... login logic
}

// Apply to registration:
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
[EnableRateLimiting("login")] // ✅ ADD THIS
public async Task<IActionResult> Register(...)
{
    // ... registration logic
}

// Apply to evidence submission:
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
[EnableRateLimiting("fileupload")] // ✅ ADD THIS
public async Task<IActionResult> ProvideTailorEvidence(...)
{
    // ... evidence submission logic
}

// Apply to email resend:
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
[EnableRateLimiting("email")] // ✅ ADD THIS
public async Task<IActionResult> ResendVerificationEmail(string email)
{
  // ... email resend logic
}
