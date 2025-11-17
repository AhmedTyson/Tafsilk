using TafsilkPlatform.Shared.Models;

namespace TafsilkPlatform.Shared.Services
{
    /// <summary>
  /// Shared data service interface that both projects can implement
    /// </summary>
    public interface IDataService
    {
        // Tailor Operations
    Task<List<TailorProfileDto>> GetAllTailorsAsync();
        Task<TailorProfileDto?> GetTailorByIdAsync(Guid id);
        Task<List<TailorProfileDto>> SearchTailorsAsync(string searchTerm, string? city = null);

     // Customer Operations
        Task<List<CustomerProfileDto>> GetAllCustomersAsync();
    Task<CustomerProfileDto?> GetCustomerByIdAsync(Guid id);

        // Service Operations
   Task<List<ServiceDto>> GetServicesByTailorIdAsync(Guid tailorId);
   Task<ServiceDto?> GetServiceByIdAsync(Guid id);

        // Order Operations
     Task<List<OrderDto>> GetAllOrdersAsync();
 Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<List<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId);
      Task<List<OrderDto>> GetOrdersByTailorIdAsync(Guid tailorId);

// Address Operations
        Task<List<AddressDto>> GetAddressesByUserIdAsync(Guid userId);
        Task<AddressDto?> GetDefaultAddressAsync(Guid userId);
    }

    /// <summary>
  /// Base implementation of shared data service
    /// Can be used by both projects or extended
    /// </summary>
    public abstract class BaseDataService : IDataService
    {
  public abstract Task<List<TailorProfileDto>> GetAllTailorsAsync();
        public abstract Task<TailorProfileDto?> GetTailorByIdAsync(Guid id);
   
   public virtual async Task<List<TailorProfileDto>> SearchTailorsAsync(string searchTerm, string? city = null)
      {
 var tailors = await GetAllTailorsAsync();
        
 if (!string.IsNullOrWhiteSpace(searchTerm))
   {
      tailors = tailors.Where(t => 
    t.ShopName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
       t.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
      t.Bio.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
 .ToList();
  }

   if (!string.IsNullOrWhiteSpace(city))
{
   tailors = tailors.Where(t => t.City.Equals(city, StringComparison.OrdinalIgnoreCase)).ToList();
     }

  return tailors;
}

     public abstract Task<List<CustomerProfileDto>> GetAllCustomersAsync();
        public abstract Task<CustomerProfileDto?> GetCustomerByIdAsync(Guid id);
  public abstract Task<List<ServiceDto>> GetServicesByTailorIdAsync(Guid tailorId);
      public abstract Task<ServiceDto?> GetServiceByIdAsync(Guid id);
   public abstract Task<List<OrderDto>> GetAllOrdersAsync();
        public abstract Task<OrderDto?> GetOrderByIdAsync(Guid id);

   public virtual async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId)
   {
       var orders = await GetAllOrdersAsync();
     return orders.Where(o => o.CustomerId == customerId).ToList();
 }

        public virtual async Task<List<OrderDto>> GetOrdersByTailorIdAsync(Guid tailorId)
{
            var orders = await GetAllOrdersAsync();
       return orders.Where(o => o.TailorId == tailorId).ToList();
        }

  public abstract Task<List<AddressDto>> GetAddressesByUserIdAsync(Guid userId);
  
        public virtual async Task<AddressDto?> GetDefaultAddressAsync(Guid userId)
   {
    var addresses = await GetAddressesByUserIdAsync(userId);
    return addresses.FirstOrDefault(a => a.IsDefault);
  }
    }
}
