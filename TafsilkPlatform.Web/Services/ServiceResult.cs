namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Generic service result wrapper for consistent response handling
/// Used across all service layer methods
/// </summary>
public class ServiceResult<T>
{
 public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private ServiceResult() { }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static ServiceResult<T> Success(T data, string? message = null)
    {
    return new ServiceResult<T>
        {
          IsSuccess = true,
            Data = data,
      Message = message
        };
    }

    /// <summary>
/// Create a failure result with single error message
/// </summary>
  public static ServiceResult<T> Failure(string errorMessage)
    {
return new ServiceResult<T>
 {
        IsSuccess = false,
  ErrorMessage = errorMessage,
            Errors = new List<string> { errorMessage }
  };
    }

    /// <summary>
    /// Create a failure result with multiple errors
    /// </summary>
    public static ServiceResult<T> Failure(List<string> errors)
    {
    return new ServiceResult<T>
 {
      IsSuccess = false,
   ErrorMessage = string.Join("; ", errors),
          Errors = errors
     };
    }
}

/// <summary>
/// Non-generic service result for void operations
/// </summary>
public class ServiceResult
{
 public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
  public string? ErrorMessage { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private ServiceResult() { }

public static ServiceResult Success(string? message = null)
    {
      return new ServiceResult
 {
        IsSuccess = true,
    Message = message
        };
    }

    public static ServiceResult Failure(string errorMessage)
    {
   return new ServiceResult
        {
  IsSuccess = false,
    ErrorMessage = errorMessage,
      Errors = new List<string> { errorMessage }
        };
    }

    public static ServiceResult Failure(List<string> errors)
  {
      return new ServiceResult
        {
            IsSuccess = false,
     ErrorMessage = string.Join("; ", errors),
          Errors = errors
        };
}
}
