using TafsilkPlatform.Models.ViewModels;

namespace TafsilkPlatform.Web.Services
{
    /// <summary>
    /// Service for managing customer and tailor profiles.
    /// </summary>
    public interface IProfileService
    {
        // Customer Profile Operations
        Task<(bool Success, string? ErrorMessage)> UpdateCustomerProfileAsync(Guid customerId, UpdateCustomerProfileRequest request);

        // Tailor Profile Operations
        Task<(bool Success, string? ErrorMessage)> UpdateTailorProfileAsync(Guid tailorId, UpdateTailorProfileRequest request);

        // Address Operations
        Task<(bool Success, string? ErrorMessage)> AddAddressAsync(Guid customerId, AddAddressRequest request);
        Task<(bool Success, string? ErrorMessage)> UpdateAddressAsync(Guid customerId, Guid addressId, EditAddressRequest request);
        Task<(bool Success, string? ErrorMessage)> DeleteAddressAsync(Guid customerId, Guid addressId);
        Task<(bool Success, string? ErrorMessage)> SetDefaultAddressAsync(Guid customerId, Guid addressId);

        // Service Operations (for Tailors)
        Task<(bool Success, string? ErrorMessage)> AddServiceAsync(Guid tailorId, AddServiceRequest request);
        Task<(bool Success, string? ErrorMessage)> UpdateServiceAsync(Guid tailorId, Guid serviceId, EditServiceRequest request);
        Task<(bool Success, string? ErrorMessage)> DeleteServiceAsync(Guid tailorId, Guid serviceId);
    }
}
