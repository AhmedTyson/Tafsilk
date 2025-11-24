namespace TafsilkPlatform.Models.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; } = 500;
    public string? Message { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    public string StatusCodeMessage => StatusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        500 => "Internal Server Error",
        503 => "Service Unavailable",
        _ => "An error occurred"
    };
}
