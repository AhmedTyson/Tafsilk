using FluentValidation.Results;
using TafsilkPlatform.Models.ViewModels;

namespace TafsilkPlatform.Web.Services
{
    /// <summary>
    /// Service for validating user input and business logic.
    /// </summary>
    public interface IValidationService
    {
        Task<ValidationResult> ValidateCustomerProfileAsync(UpdateCustomerProfileRequest request);
        Task<ValidationResult> ValidateTailorProfileAsync(UpdateTailorProfileRequest request);
        Task<ValidationResult> ValidateAddressAsync(AddAddressRequest request);
        Task<ValidationResult> ValidateServiceAsync(AddServiceRequest request);

        /// <summary>
        /// Validates complete tailor profile request during initial registration
        /// </summary>
        Task<ValidationResult> ValidateCompleteTailorProfileAsync(CompleteTailorProfileRequest request);
    }
}
