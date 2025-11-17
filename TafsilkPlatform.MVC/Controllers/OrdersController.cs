using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.MVC.Services;

namespace TafsilkPlatform.MVC.Controllers
{
 [Authorize]
    public class OrdersController : Controller
    {
   private readonly MockDataService _mockDataService;

        public OrdersController(MockDataService mockDataService)
   {
  _mockDataService = mockDataService;
 }

   // GET: Orders
   public IActionResult Index()
        {
   var orders = _mockDataService.GetAllOrders();
      return View(orders);
      }

  // GET: Orders/Details/5
        public IActionResult Details(Guid id)
 {
   var order = _mockDataService.GetOrderById(id);
   if (order == null)
      return NotFound();

    return View(order);
 }

  // GET: Orders/MyOrders
  public IActionResult MyOrders()
        {
     // In a real app, get current user's ID from claims
     var customerId = _mockDataService.GetAllCustomers().FirstOrDefault()?.Id ?? Guid.Empty;
  var orders = _mockDataService.GetOrdersByCustomerId(customerId);
   return View(orders);
  }
    }
}
