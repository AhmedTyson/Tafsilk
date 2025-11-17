using TafsilkPlatform.MVC.Models;

namespace TafsilkPlatform.MVC.Services
{
    /// <summary>
    /// Mock data service - provides fake/static data for demonstration
    /// </summary>
    public class MockDataService
 {
    private static readonly List<TailorProfile> _tailors = new()
        {
      new TailorProfile
     {
 Id = Guid.NewGuid(),
         ShopName = "ورشة الأناقة",
      FullName = "محمد الخياط",
     Bio = "خياط محترف بخبرة 15 عاماً في تفصيل الملابس الرجالية والنسائية",
        City = "القاهرة",
           Address = "شارع فيصل، الجيزة",
  ExperienceYears = 15,
         Rating = 4.8m,
      ReviewCount = 124,
  ImageUrl = "/images/tailor1.jpg",
   Specialties = new List<string> { "بدلات رجالية", "فساتين سهرة", "عبايات" }
      },
  new TailorProfile
     {
  Id = Guid.NewGuid(),
      ShopName = "التفصيل الراقي",
     FullName = "أحمد حسن",
     Bio = "متخصص في التفصيل الحديث والتصاميم العصرية",
    City = "الإسكندرية",
   Address = "شارع الحرية، سموحة",
      ExperienceYears = 10,
     Rating = 4.5m,
     ReviewCount = 87,
ImageUrl = "/images/tailor2.jpg",
   Specialties = new List<string> { "قمصان", "بناطيل", "جلاليب" }
   },
   new TailorProfile
 {
     Id = Guid.NewGuid(),
        ShopName = "خياطة الفخامة",
        FullName = "سارة أحمد",
       Bio = "خياطة نسائية متخصصة في الفساتين والعبايات الفاخرة",
  City = "الرياض",
   Address = "حي العليا، الرياض",
    ExperienceYears = 8,
      Rating = 4.9m,
    ReviewCount = 156,
     ImageUrl = "/images/tailor3.jpg",
     Specialties = new List<string> { "فساتين زفاف", "عبايات فاخرة", "جلابيات" }
   }
        };

    private static readonly List<CustomerProfile> _customers = new()
      {
       new CustomerProfile
  {
   Id = Guid.NewGuid(),
        FullName = "أحمد محمد علي",
        Email = "ahmed@example.com",
       PhoneNumber = "01012345678",
City = "القاهرة",
       OrderCount = 5,
    JoinedAt = DateTime.UtcNow.AddMonths(-6)
          },
   new CustomerProfile
{
  Id = Guid.NewGuid(),
         FullName = "فاطمة حسن",
    Email = "fatima@example.com",
      PhoneNumber = "01098765432",
       City = "الإسكندرية",
       OrderCount = 3,
          JoinedAt = DateTime.UtcNow.AddMonths(-4)
            },
   new CustomerProfile
      {
       Id = Guid.NewGuid(),
        FullName = "عمر خالد",
   Email = "omar@example.com",
         PhoneNumber = "01155667788",
     City = "الجيزة",
  OrderCount = 8,
    JoinedAt = DateTime.UtcNow.AddMonths(-12)
  }
        };

        private static readonly List<TailorService> _services = new()
   {
   new TailorService
   {
     Id = Guid.NewGuid(),
   TailorId = _tailors[0].Id,
    ServiceName = "تفصيل بدلة رجالية",
      Description = "بدلة رجالية كاملة بقياسات دقيقة",
       BasePrice = 1200m,
      EstimatedDays = 7,
       Category = "ملابس رجالية"
            },
       new TailorService
    {
     Id = Guid.NewGuid(),
   TailorId = _tailors[0].Id,
        ServiceName = "تفصيل فستان سهرة",
    Description = "فستان سهرة حسب التصميم المطلوب",
           BasePrice = 1500m,
  EstimatedDays = 10,
        Category = "ملابس نسائية"
   },
            new TailorService
   {
    Id = Guid.NewGuid(),
    TailorId = _tailors[1].Id,
        ServiceName = "تفصيل قميص",
  Description = "قميص رجالي بقياسات مخصصة",
         BasePrice = 300m,
        EstimatedDays = 3,
   Category = "ملابس رجالية"
   },
 new TailorService
       {
      Id = Guid.NewGuid(),
   TailorId = _tailors[2].Id,
       ServiceName = "تفصيل عباية فاخرة",
     Description = "عباية مطرزة بتصاميم حصرية",
    BasePrice = 2000m,
   EstimatedDays = 14,
   Category = "ملابس نسائية"
}
 };

  private static readonly List<Order> _orders = new()
{
            new Order
       {
         Id = Guid.NewGuid(),
  CustomerId = _customers[0].Id,
         CustomerName = _customers[0].FullName,
     TailorId = _tailors[0].Id,
   TailorName = _tailors[0].ShopName,
   ServiceName = "تفصيل بدلة رجالية",
      TotalPrice = 1200m,
        Status = "قيد التنفيذ",
    OrderDate = DateTime.UtcNow.AddDays(-5),
      DeliveryDate = DateTime.UtcNow.AddDays(2)
  },
   new Order
     {
 Id = Guid.NewGuid(),
   CustomerId = _customers[1].Id,
  CustomerName = _customers[1].FullName,
 TailorId = _tailors[2].Id,
   TailorName = _tailors[2].ShopName,
ServiceName = "تفصيل عباية فاخرة",
          TotalPrice = 2000m,
   Status = "مكتمل",
       OrderDate = DateTime.UtcNow.AddDays(-20),
  DeliveryDate = DateTime.UtcNow.AddDays(-6)
     },
   new Order
{
Id = Guid.NewGuid(),
      CustomerId = _customers[2].Id,
   CustomerName = _customers[2].FullName,
   TailorId = _tailors[1].Id,
     TailorName = _tailors[1].ShopName,
      ServiceName = "تفصيل قميص",
    TotalPrice = 300m,
     Status = "جديد",
     OrderDate = DateTime.UtcNow.AddDays(-1),
           DeliveryDate = DateTime.UtcNow.AddDays(2)
  }
        };

 public List<TailorProfile> GetAllTailors() => _tailors;
        public TailorProfile? GetTailorById(Guid id) => _tailors.FirstOrDefault(t => t.Id == id);

    public List<CustomerProfile> GetAllCustomers() => _customers;
        public CustomerProfile? GetCustomerById(Guid id) => _customers.FirstOrDefault(c => c.Id == id);

   public List<TailorService> GetServicesByTailorId(Guid tailorId) => 
       _services.Where(s => s.TailorId == tailorId).ToList();
        
        public List<TailorService> GetAllServices() => _services;
  public TailorService? GetServiceById(Guid id) => _services.FirstOrDefault(s => s.Id == id);

 public List<Order> GetAllOrders() => _orders;
        public Order? GetOrderById(Guid id) => _orders.FirstOrDefault(o => o.Id == id);
        public List<Order> GetOrdersByCustomerId(Guid customerId) => 
  _orders.Where(o => o.CustomerId == customerId).ToList();
        public List<Order> GetOrdersByTailorId(Guid tailorId) => 
   _orders.Where(o => o.TailorId == tailorId).ToList();

     public DashboardStats GetDashboardStats() => new()
        {
      TotalOrders = _orders.Count,
       PendingOrders = _orders.Count(o => o.Status == "قيد التنفيذ" || o.Status == "جديد"),
     CompletedOrders = _orders.Count(o => o.Status == "مكتمل"),
            TotalRevenue = _orders.Where(o => o.Status == "مكتمل").Sum(o => o.TotalPrice),
   TotalCustomers = _customers.Count,
 TotalTailors = _tailors.Count
     };
    }
}
