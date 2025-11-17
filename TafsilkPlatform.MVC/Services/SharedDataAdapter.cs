using TafsilkPlatform.Shared.Models;
using TafsilkPlatform.Shared.Services;
using TafsilkPlatform.MVC.Models;

namespace TafsilkPlatform.MVC.Services
{
    /// <summary>
    /// Adapter to integrate MVC's MockDataService with shared IDataService interface
    /// </summary>
    public class SharedDataAdapter : BaseDataService
    {
        private readonly MockDataService _mockDataService;

    public SharedDataAdapter(MockDataService mockDataService)
        {
          _mockDataService = mockDataService;
        }

     public override async Task<List<TailorProfileDto>> GetAllTailorsAsync()
        {
          await Task.CompletedTask;
  
        return _mockDataService.GetAllTailors().Select(t => new TailorProfileDto
        {
                Id = t.Id,
          UserId = Guid.NewGuid(), // Mock user ID
       ShopName = t.ShopName,
      FullName = t.FullName,
Bio = t.Bio,
           City = t.City,
                Address = t.Address,
                ExperienceYears = t.ExperienceYears,
        Rating = t.Rating,
          ReviewCount = t.ReviewCount,
    Specialties = t.Specialties,
       CreatedAt = DateTime.UtcNow.AddMonths(-t.ExperienceYears * 12),
        UpdatedAt = DateTime.UtcNow
            }).ToList();
    }

 public override async Task<TailorProfileDto?> GetTailorByIdAsync(Guid id)
    {
            await Task.CompletedTask;
        
    var tailor = _mockDataService.GetTailorById(id);
if (tailor == null) return null;

            return new TailorProfileDto
            {
     Id = tailor.Id,
      UserId = Guid.NewGuid(),
                ShopName = tailor.ShopName,
    FullName = tailor.FullName,
       Bio = tailor.Bio,
      City = tailor.City,
     Address = tailor.Address,
      ExperienceYears = tailor.ExperienceYears,
      Rating = tailor.Rating,
         ReviewCount = tailor.ReviewCount,
                Specialties = tailor.Specialties,
        CreatedAt = DateTime.UtcNow.AddMonths(-tailor.ExperienceYears * 12),
       UpdatedAt = DateTime.UtcNow
            };
        }

   public override async Task<List<CustomerProfileDto>> GetAllCustomersAsync()
        {
 await Task.CompletedTask;
        
     return _mockDataService.GetAllCustomers().Select(c => new CustomerProfileDto
          {
                Id = c.Id,
      UserId = Guid.NewGuid(),
    FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                City = c.City,
    OrderCount = c.OrderCount,
    JoinedAt = c.JoinedAt
            }).ToList();
        }

  public override async Task<CustomerProfileDto?> GetCustomerByIdAsync(Guid id)
  {
      await Task.CompletedTask;
            
   var customer = _mockDataService.GetCustomerById(id);
            if (customer == null) return null;

  return new CustomerProfileDto
            {
  Id = customer.Id,
        UserId = Guid.NewGuid(),
         FullName = customer.FullName,
        Email = customer.Email,
    PhoneNumber = customer.PhoneNumber,
  City = customer.City,
      OrderCount = customer.OrderCount,
                JoinedAt = customer.JoinedAt
         };
        }

    public override async Task<List<ServiceDto>> GetServicesByTailorIdAsync(Guid tailorId)
        {
    await Task.CompletedTask;
            
  return _mockDataService.GetServicesByTailorId(tailorId).Select(s => new ServiceDto
        {
      Id = s.Id,
      TailorId = s.TailorId,
             ServiceName = s.ServiceName,
       Description = s.Description,
       BasePrice = s.BasePrice,
        EstimatedDays = s.EstimatedDays,
  Category = s.Category,
           IsActive = true
    }).ToList();
   }

        public override async Task<ServiceDto?> GetServiceByIdAsync(Guid id)
        {
    await Task.CompletedTask;
         
   var service = _mockDataService.GetServiceById(id);
            if (service == null) return null;

      return new ServiceDto
        {
   Id = service.Id,
       TailorId = service.TailorId,
      ServiceName = service.ServiceName,
      Description = service.Description,
          BasePrice = service.BasePrice,
  EstimatedDays = service.EstimatedDays,
                Category = service.Category,
     IsActive = true
 };
  }

        public override async Task<List<OrderDto>> GetAllOrdersAsync()
     {
          await Task.CompletedTask;
            
        return _mockDataService.GetAllOrders().Select(o => new OrderDto
            {
                Id = o.Id,
   CustomerId = o.CustomerId,
     CustomerName = o.CustomerName,
             TailorId = o.TailorId,
 TailorName = o.TailorName,
                ServiceName = o.ServiceName,
                TotalPrice = o.TotalPrice,
       Status = o.Status,
         OrderDate = o.OrderDate,
         DeliveryDate = o.DeliveryDate
            }).ToList();
        }

        public override async Task<OrderDto?> GetOrderByIdAsync(Guid id)
    {
          await Task.CompletedTask;
  
    var order = _mockDataService.GetOrderById(id);
            if (order == null) return null;

  return new OrderDto
     {
    Id = order.Id,
   CustomerId = order.CustomerId,
         CustomerName = order.CustomerName,
        TailorId = order.TailorId,
           TailorName = order.TailorName,
  ServiceName = order.ServiceName,
    TotalPrice = order.TotalPrice,
      Status = order.Status,
    OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate
            };
        }

   public override async Task<List<AddressDto>> GetAddressesByUserIdAsync(Guid userId)
 {
            await Task.CompletedTask;
   // Mock implementation - return empty list
     return new List<AddressDto>();
        }
}
}
