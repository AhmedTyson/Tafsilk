using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.DataAccess.Repository;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels;

namespace TafsilkPlatform.Web.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
      IUnitOfWork unitOfWork,
  ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ==================== PRIVATE HELPER METHODS ====================

        /// <summary>
        /// Verifies ownership of an entity by checking if the userId matches.
        /// </summary>
        private bool VerifyOwnership(Guid entityUserId, Guid currentUserId)
        {
            return entityUserId == currentUserId;
        }

        /// <summary>
        /// Unsets all default flags for user addresses except the specified one.
        /// </summary>
        private async Task UnsetOtherDefaultAddressesAsync(Guid userId, Guid? exceptAddressId = null)
        {
            var existingDefaults = await _unitOfWork.Context.UserAddresses
  .Where(a => a.UserId == userId && a.IsDefault && a.Id != exceptAddressId)
     .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }
        }

        /// <summary>
        /// Reassigns default flag to another address when the current default is being deleted.
        /// </summary>
        private async Task ReassignDefaultAddressAsync(Guid userId, Guid deletedAddressId)
        {
            var anotherAddress = await _unitOfWork.Context.UserAddresses
              .Where(a => a.UserId == userId && a.Id != deletedAddressId)
              .FirstOrDefaultAsync();

            if (anotherAddress != null)
            {
                anotherAddress.IsDefault = true;
            }
        }

        // ==================== CUSTOMER PROFILE ====================

        /// <summary>
        /// Updates customer profile with validation.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateCustomerProfileAsync(
          Guid customerId,
      UpdateCustomerProfileRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Updating customer profile: {CustomerId}", customerId);

                var profile = await _unitOfWork.Customers
                        .GetByUserIdAsync(customerId);

                if (profile == null)
                {
                    _logger.LogWarning("[ProfileService] Customer profile not found: {CustomerId}", customerId);
                    return (false, "الملف الشخصي غير موجود");
                }

                // Update fields
                profile.FullName = request.FullName;
                profile.Gender = request.Gender;
                profile.City = request.City;
                profile.DateOfBirth = request.DateOfBirth.HasValue ?
                 DateOnly.FromDateTime(request.DateOfBirth.Value) : null;
                profile.Bio = request.Bio;
                profile.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Customer profile updated successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error updating customer profile");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ==================== TAILOR PROFILE ====================

        /// <summary>
        /// Updates tailor profile with validation.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateTailorProfileAsync(
           Guid tailorId,
            UpdateTailorProfileRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Updating tailor profile: {TailorId}", tailorId);

                var profile = await _unitOfWork.Tailors
         .GetByUserIdAsync(tailorId);

                if (profile == null)
                {
                    _logger.LogWarning("[ProfileService] Tailor profile not found: {TailorId}", tailorId);
                    return (false, "الملف الشخصي غير موجود");
                }

                // Update fields
                profile.ShopName = request.ShopName;
                profile.FullName = request.FullName;
                profile.Bio = request.Bio;
                profile.Address = request.Address;
                profile.City = request.City;
                profile.ExperienceYears = request.ExperienceYears;
                profile.PricingRange = request.PricingRange;
                profile.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Tailor profile updated successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error updating tailor profile");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ==================== ADDRESS OPERATIONS ====================

        /// <summary>
        /// Adds new delivery address for customer.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AddAddressAsync(
            Guid customerId,
      AddAddressRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Adding address for customer: {CustomerId}", customerId);

                var profile = await _unitOfWork.Customers.GetByUserIdAsync(customerId);
                if (profile == null)
                {
                    return (false, "الملف الشخصي غير موجود");
                }

                var address = new UserAddress
                {
                    UserId = customerId,
                    Label = request.Label,
                    Street = request.StreetAddress,
                    City = request.City,
                    IsDefault = request.IsDefault,
                    Latitude = request.Latitude.HasValue ? (decimal?)request.Latitude.Value : null,
                    Longitude = request.Longitude.HasValue ? (decimal?)request.Longitude.Value : null,
                    CreatedAt = DateTime.UtcNow
                };

                // If this is default, unset other defaults
                if (request.IsDefault)
                {
                    await UnsetOtherDefaultAddressesAsync(customerId);
                }

                await _unitOfWork.Addresses.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Address added successfully: {AddressId}", address.Id);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error adding address");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates existing address.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateAddressAsync(
                    Guid customerId,
         Guid addressId,
              EditAddressRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Updating address: {AddressId}", addressId);

                var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);
                if (address == null)
                {
                    return (false, "العنوان غير موجود");
                }

                // Verify ownership using helper method
                if (!VerifyOwnership(address.UserId, customerId))
                {
                    return (false, "غير مصرح بهذا الإجراء");
                }

                // Update fields
                address.Label = request.Label;
                address.Street = request.StreetAddress;
                address.City = request.City;
                address.Latitude = request.Latitude.HasValue ? (decimal?)request.Latitude.Value : null;
                address.Longitude = request.Longitude.HasValue ? (decimal?)request.Longitude.Value : null;

                // Handle default change
                if (request.IsDefault && !address.IsDefault)
                {
                    await UnsetOtherDefaultAddressesAsync(customerId, addressId);
                    address.IsDefault = true;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Address updated successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error updating address");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a delivery address.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteAddressAsync(
  Guid customerId,
            Guid addressId)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Deleting address: {AddressId}", addressId);

                var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);
                if (address == null)
                {
                    return (false, "العنوان غير موجود");
                }

                // Verify ownership using helper method
                if (!VerifyOwnership(address.UserId, customerId))
                {
                    return (false, "غير مصرح بهذا الإجراء");
                }

                // If deleting default address, make another one default
                if (address.IsDefault)
                {
                    await ReassignDefaultAddressAsync(customerId, addressId);
                }

                await _unitOfWork.Addresses.DeleteAsync(address);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Address deleted successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error deleting address");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets address as default.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> SetDefaultAddressAsync(
    Guid customerId,
            Guid addressId)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Setting default address: {AddressId}", addressId);

                var address = await _unitOfWork.Addresses.GetByIdAsync(addressId);
                if (address == null)
                {
                    return (false, "العنوان غير موجود");
                }

                // Verify ownership using helper method
                if (!VerifyOwnership(address.UserId, customerId))
                {
                    return (false, "غير مصرح بهذا الإجراء");
                }

                // Unset other defaults
                await UnsetOtherDefaultAddressesAsync(customerId, addressId);
                address.IsDefault = true;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Default address set successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error setting default address");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        // ==================== SERVICE OPERATIONS ====================

        /// <summary>
        /// Adds new service for tailor.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AddServiceAsync(
      Guid tailorId,
           AddServiceRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Adding service for tailor: {TailorId}", tailorId);

                var profile = await _unitOfWork.Tailors.GetByUserIdAsync(tailorId);
                if (profile == null)
                {
                    return (false, "الملف الشخصي غير موجود");
                }

                var service = new TailorService
                {
                    TailorServiceId = Guid.NewGuid(),
                    TailorId = profile.Id,
                    ServiceName = request.ServiceName,
                    Description = request.Description,
                    BasePrice = request.BasePrice,
                    EstimatedDuration = request.EstimatedDuration,
                    IsDeleted = false
                };

                await _unitOfWork.TailorServices.AddAsync(service);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Service added successfully: {ServiceId}", service.TailorServiceId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error adding service");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates existing service.
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateServiceAsync(
      Guid tailorId,
 Guid serviceId,
     EditServiceRequest request)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Updating service: {ServiceId}", serviceId);

                var service = await _unitOfWork.TailorServices.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return (false, "الخدمة غير موجودة");
                }

                // Verify ownership
                var profile = await _unitOfWork.Tailors.GetByUserIdAsync(tailorId);
                if (profile == null || !VerifyOwnership(service.TailorId, profile.Id))
                {
                    return (false, "غير مصرح بهذا الإجراء");
                }

                // Update fields
                service.ServiceName = request.ServiceName;
                service.Description = request.Description;
                service.BasePrice = request.BasePrice;
                service.EstimatedDuration = request.EstimatedDuration;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Service updated successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error updating service");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a service (soft delete).
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> DeleteServiceAsync(
            Guid tailorId,
     Guid serviceId)
        {
            try
            {
                _logger.LogInformation("[ProfileService] Deleting service: {ServiceId}", serviceId);

                var service = await _unitOfWork.TailorServices.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return (false, "الخدمة غير موجودة");
                }

                // Verify ownership
                var profile = await _unitOfWork.Tailors.GetByUserIdAsync(tailorId);
                if (profile == null || !VerifyOwnership(service.TailorId, profile.Id))
                {
                    return (false, "غير مصرح بهذا الإجراء");
                }

                // Soft delete
                service.IsDeleted = true;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[ProfileService] Service deleted successfully");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProfileService] Error deleting service");
                return (false, $"حدث خطأ: {ex.Message}");
            }
        }
    }
}
