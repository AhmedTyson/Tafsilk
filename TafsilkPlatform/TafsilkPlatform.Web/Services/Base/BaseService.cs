using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Common;

namespace TafsilkPlatform.Web.Services.Base;

/// <summary>
/// Base service class with common patterns for all services
/// Provides consistent error handling, validation, and transaction management
/// </summary>
public abstract class BaseService
{
    protected readonly ILogger Logger;

    protected BaseService(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute operation with automatic transaction management and error handling
    /// </summary>
    protected async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        Guid? userId = null)
    {
        try
        {
            Logger.LogInformation(
                "[{Service}] Starting {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            var result = await operation();

            Logger.LogInformation(
                "[{Service}] Completed {Operation} successfully for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Success(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Concurrency conflict in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure("The data was modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex,
                "[{Service}] Database error in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure("A database error occurred. Please try again or contact support.");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Invalid operation in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Invalid argument in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Unauthorized access in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure("You are not authorized to perform this action.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "[{Service}] Unexpected error in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result<T>.Failure("An unexpected error occurred. Please try again or contact support.");
        }
    }

    /// <summary>
    /// Execute operation without return value
    /// </summary>
    protected async Task<Result> ExecuteAsync(
        Func<Task> operation,
        string operationName,
        Guid? userId = null)
    {
        try
        {
            Logger.LogInformation(
                "[{Service}] Starting {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            await operation();

            Logger.LogInformation(
                "[{Service}] Completed {Operation} successfully for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Concurrency conflict in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure("The data was modified by another process. Please refresh and try again.");
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex,
                "[{Service}] Database error in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure("A database error occurred. Please try again or contact support.");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Invalid operation in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Invalid argument in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex,
                "[{Service}] Unauthorized access in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure("You are not authorized to perform this action.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "[{Service}] Unexpected error in {Operation} for user {UserId}",
                GetType().Name, operationName, userId);

            return Result.Failure("An unexpected error occurred. Please try again or contact support.");
        }
    }

    /// <summary>
    /// Validate required parameter
    /// </summary>
    protected void ValidateRequired<T>(T value, string paramName) where T : class?
    {
        if (value == null)
        {
            throw new ArgumentException($"{paramName} is required", paramName);
        }
    }

    /// <summary>
    /// Validate GUID is not empty
    /// </summary>
    protected void ValidateGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }

    /// <summary>
    /// Validate positive number
    /// </summary>
    protected void ValidatePositive(decimal value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        }
    }

    /// <summary>
    /// Validate non-negative number
    /// </summary>
    protected void ValidateNonNegative(decimal value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{paramName} must be zero or greater", paramName);
        }
    }

    /// <summary>
    /// Validate non-negative integer
    /// </summary>
    protected void ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentException($"{paramName} must be zero or greater", paramName);
        }
    }

    /// <summary>
    /// Validate string is not empty
    /// </summary>
    protected void ValidateNotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }

    /// <summary>
    /// Validate collection is not empty
    /// </summary>
    protected void ValidateNotEmpty<T>(IEnumerable<T>? collection, string paramName)
    {
        if (collection == null || !collection.Any())
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }

    /// <summary>
    /// Validate range
    /// </summary>
    protected void ValidateRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentException($"{paramName} must be between {min} and {max}", paramName);
        }
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    protected void ValidateEmail(string? email, string paramName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            throw new ArgumentException($"{paramName} is not a valid email address", paramName);
        }
    }
}
