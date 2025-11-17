using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.MVC.Services;

namespace TafsilkPlatform.MVC.Controllers
{
    public class TailorsController : Controller
    {
        private readonly MockDataService _mockDataService;

        public TailorsController(MockDataService mockDataService)
      {
  _mockDataService = mockDataService;
        }

        // GET: Tailors
        public IActionResult Index()
        {
 var tailors = _mockDataService.GetAllTailors();
  return View(tailors);
        }

   // GET: Tailors/Details/5
        public IActionResult Details(Guid id)
 {
 var tailor = _mockDataService.GetTailorById(id);
 if (tailor == null)
  return NotFound();

  var services = _mockDataService.GetServicesByTailorId(id);
         ViewBag.Services = services;

          return View(tailor);
        }

  // GET: Tailors/Services/5
  public IActionResult Services(Guid id)
 {
   var tailor = _mockDataService.GetTailorById(id);
     if (tailor == null)
  return NotFound();

  var services = _mockDataService.GetServicesByTailorId(id);
   ViewBag.TailorName = tailor.ShopName;

    return View(services);
 }
 }
}
