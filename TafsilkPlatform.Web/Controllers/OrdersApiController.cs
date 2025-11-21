using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.Orders;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// API Controller for idempotent order creation
/// Supports Idempotency-Key header to prevent duplicate orders
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersApiController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IIdempotencyStore _idempotencyStore;
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    public OrdersApiController(
        IOrderService orderService,
        IIdempotencyStore idempotencyStore,
        ILogger<OrdersApiController> logger) : base(logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _idempotencyStore = idempotencyStore ?? throw new ArgumentNullException(nameof(idempotencyStore));
    }

    /// <summary>
    /// Create a new order with idempotency support
    /// POST /api/orders
    /// </summary>
    /// <param name="model">Order creation request</param>
    /// <returns>Order creation result</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Request is currently being processed (duplicate key)</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
    {
        // ==================== STEP 1: VALIDATE IDEMPOTENCY KEY ====================

        var idempotencyKey = Request.Headers[IdempotencyKeyHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            _logger.LogWarning("[OrdersApi] Request without Idempotency-Key header");
            return BadRequest(new
            {
                success = false,
                message = "Idempotency-Key header is required for order creation",
                error = "MISSING_IDEMPOTENCY_KEY"
            });
        }

        // Validate key format (max 128 characters, alphanumeric with hyphens)
        if (idempotencyKey.Length > 128 || !IsValidIdempotencyKey(idempotencyKey))
        {
            _logger.LogWarning("[OrdersApi] Invalid Idempotency-Key format: {Key}", idempotencyKey);
            return BadRequest(new
            {
                success = false,
                message = "Invalid Idempotency-Key format. Use alphanumeric characters, hyphens, or underscores (max 128 chars)",
                error = "INVALID_IDEMPOTENCY_KEY"
            });
        }

        var userId = GetUserId();
        var endpoint = $"{Request.Method} {Request.Path}";

        _logger.LogInformation("[OrdersApi] Received order creation request with Idempotency-Key: {Key}, User: {UserId}",
            idempotencyKey, userId);

        // ==================== STEP 2: CHECK FOR EXISTING RESPONSE ====================

        var (found, cachedResult, statusCode) = await _idempotencyStore.TryGetResponseAsync(idempotencyKey);

        if (found && cachedResult != null)
        {
            _logger.LogInformation("[OrdersApi] Returning cached response for Idempotency-Key: {Key}, StatusCode: {StatusCode}",
      idempotencyKey, statusCode);

            // Return the cached response with the same status code
            return StatusCode(statusCode ?? 200, cachedResult);
        }

        // Special case: Request is currently being processed
        if (!found && statusCode == 409)
        {
            _logger.LogWarning("[OrdersApi] Duplicate request detected (in progress): {Key}", idempotencyKey);
            return Conflict(new
            {
                success = false,
                message = "A request with this Idempotency-Key is currently being processed. Please try again in a few seconds.",
                error = "REQUEST_IN_PROGRESS"
            });
        }

        // ==================== STEP 3: MARK AS IN PROGRESS ====================

        var marked = await _idempotencyStore.TryMarkAsInProgressAsync(
      idempotencyKey,
   userId,
 endpoint,
            Request.Method);

        if (!marked)
        {
            // Race condition: Another request with the same key started processing
            _logger.LogWarning("[OrdersApi] Concurrent request detected for Idempotency-Key: {Key}", idempotencyKey);
            return Conflict(new
            {
                success = false,
                message = "A concurrent request with this Idempotency-Key is being processed. Please retry.",
                error = "CONCURRENT_REQUEST"
            });
        }

        try
        {
            // ==================== STEP 4: VALIDATE REQUEST ====================

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                 .SelectMany(v => v.Errors)
               .Select(e => e.ErrorMessage)
                      .ToList();

                _logger.LogWarning("[OrdersApi] Invalid model state for Idempotency-Key: {Key}, Errors: {Errors}",
                     idempotencyKey, string.Join(", ", errors));

                var errorResult = new OrderResult
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };

                // Save error response
                await _idempotencyStore.TrySaveResponseAsync(
                     idempotencyKey,
                      errorResult,
                      400,
                         userId,
                  endpoint,
                       Request.Method);

                return BadRequest(errorResult);
            }

            // ==================== STEP 5: EXECUTE BUSINESS LOGIC ====================

            _logger.LogInformation("[OrdersApi] Executing order creation for Idempotency-Key: {Key}, User: {UserId}",
     idempotencyKey, userId);

            var result = await _orderService.CreateOrderWithResultAsync(model, userId);

            // ==================== STEP 6: SAVE AND RETURN RESPONSE ====================

            var responseStatusCode = result.Success ? 200 : 400;

            var saved = await _idempotencyStore.TrySaveResponseAsync(
                         idempotencyKey,
                  result,
              responseStatusCode,
               userId,
                endpoint,
                      Request.Method);

            if (!saved)
            {
                _logger.LogWarning("[OrdersApi] Failed to save response for Idempotency-Key: {Key}", idempotencyKey);
            }

            _logger.LogInformation("[OrdersApi] Order creation completed for Idempotency-Key: {Key}, Success: {Success}, OrderId: {OrderId}",
                 idempotencyKey, result.Success, result.OrderId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            // ==================== ERROR HANDLING ====================

            _logger.LogError(ex, "[OrdersApi] Error processing order for Idempotency-Key: {Key}", idempotencyKey);

            // Mark as failed
            await _idempotencyStore.MarkAsFailedAsync(idempotencyKey, ex.Message);

            var errorResult = new OrderResult
            {
                Success = false,
                Message = "An unexpected error occurred while creating the order",
                Errors = new List<string> { ex.Message }
            };

            return StatusCode(500, errorResult);
        }
    }

    /// <summary>
    /// Get order status by Idempotency-Key
    /// GET /api/orders/status/{idempotencyKey}
    /// </summary>
    [HttpGet("status/{idempotencyKey}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderStatus(string idempotencyKey)
    {
        _logger.LogInformation("[OrdersApi] Checking status for Idempotency-Key: {Key}", idempotencyKey);

        var (found, result, statusCode) = await _idempotencyStore.TryGetResponseAsync(idempotencyKey);

        if (!found)
        {
            var isInProgress = await _idempotencyStore.IsInProgressAsync(idempotencyKey);

            if (isInProgress)
            {
                return Ok(new
                {
                    status = "in_progress",
                    message = "Request is currently being processed"
                });
            }

            return NotFound(new
            {
                status = "not_found",
                message = "No request found with this Idempotency-Key"
            });
        }

        return StatusCode(statusCode ?? 200, result);
    }

    /// <summary>
    /// Validate idempotency key format
    /// </summary>
    private bool IsValidIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        // Allow alphanumeric, hyphens, underscores
        return key.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}

/// <summary>
/// Background service for cleaning up expired idempotency keys
/// </summary>
public class IdempotencyCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdempotencyCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

    public IdempotencyCleanupService(
        IServiceProvider serviceProvider,
      ILogger<IdempotencyCleanupService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[IdempotencyCleanup] Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                _logger.LogInformation("[IdempotencyCleanup] Running cleanup job");

                using var scope = _serviceProvider.CreateScope();
                var idempotencyStore = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

                var deletedCount = await idempotencyStore.CleanupExpiredKeysAsync();

                if (deletedCount > 0)
                {
                    _logger.LogInformation("[IdempotencyCleanup] Cleaned up {Count} expired keys", deletedCount);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IdempotencyCleanup] Error during cleanup job");
            }
        }

        _logger.LogInformation("[IdempotencyCleanup] Service stopped");
    }
}
