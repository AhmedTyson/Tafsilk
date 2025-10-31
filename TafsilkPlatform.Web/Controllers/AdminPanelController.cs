using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TafsilkPlatform.Web.Controllers
{
 [Authorize(Roles = "Admin")]
 public class AdminPanelController : Controller
 {
 // Render the admin dashboard page (server view)
 public IActionResult Index()
 {
 // Returns the existing admin dashboard view file
 return View("~/Views/Dashboards/admindashborad.cshtml");
 }
 }
}
