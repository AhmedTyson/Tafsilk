using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.MVC.Services;

namespace TafsilkPlatform.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly MockDataService _mockDataService;

        public DashboardController(MockDataService mockDataService)
   {
 _mockDataService = mockDataService;
        }

 // GET: Dashboard
  public IActionResult Index()
 {
       var stats = _mockDataService.GetDashboardStats();
   return View(stats);
 }

  // GET: Dashboard/Customers
 public IActionResult Customers()
        {
 var customers = _mockDataService.GetAllCustomers();
   return View(customers);
  }

  // GET: Dashboard/Tailors
        public IActionResult Tailors()
 {
       var tailors = _mockDataService.GetAllTailors();
      return View(tailors);
 }

        // GET: Dashboard/Orders
    public IActionResult Orders()
  {
   var orders = _mockDataService.GetAllOrders();
 return View(orders);
        }
 }
}
