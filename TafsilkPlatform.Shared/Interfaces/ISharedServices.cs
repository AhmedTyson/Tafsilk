namespace TafsilkPlatform.Shared.Interfaces
{
    /// <summary>
    /// Shared authentication service interface
    /// </summary>
    public interface IAuthenticationService
  {
        Task<(bool Success, string? ErrorMessage, string? UserId)> AuthenticateAsync(string email, string password);
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(string email, string password, string fullName, string role);
        Task<bool> ValidateUserAsync(string userId);
    }

    /// <summary>
    /// Shared profile service interface
    /// </summary>
    public interface IProfileManagementService
 {
      Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, string city);
        Task<object?> GetProfileAsync(string userId);
}

    /// <summary>
    /// Shared service for common operations
    /// </summary>
    public interface ICommonService
    {
    Task<List<string>> GetCitiesAsync();
   Task<List<string>> GetSpecialtiesAsync();
        string GenerateId();
        DateTime GetCurrentDateTime();
    }
}
