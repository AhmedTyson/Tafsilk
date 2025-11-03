namespace TafsilkPlatform.Web.Common;

/// <summary>
/// Generic result pattern for operation outcomes
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
  public T? Value { get; }
    public string? Error { get; }
    public Dictionary<string, List<string>>? ValidationErrors { get; }

    private Result(bool isSuccess, T? value, string? error, Dictionary<string, List<string>>? validationErrors = null)
    {
        IsSuccess = isSuccess;
      Value = value;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    
    public static Result<T> Failure(string error) => new(false, default, error);
    
    public static Result<T> ValidationFailure(Dictionary<string, List<string>> errors) 
        => new(false, default, "خطأ في التحقق من البيانات", errors);
}

/// <summary>
/// Non-generic result for operations without return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
   IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    
    public static Result Failure(string error) => new(false, error);
}
