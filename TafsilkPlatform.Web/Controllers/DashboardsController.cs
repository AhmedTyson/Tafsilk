using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Controllers;

[Authorize]
public class DashboardsController : Controller
{
 [Authorize(Roles = "Customer")]
 public IActionResult Customer()
 {
 ViewData["Title"] = "لوحة العميل";
 return View();
 }

 [Authorize(Roles = "Corporate")]
 public IActionResult Corporate()
 {
 ViewData["Title"] = "لوحة الشركة";
 return View();
 }

 [Authorize(Roles = "Tailor")]
 public IActionResult Tailor()
 {
 ViewData["Title"] = "لوحة الخياط";
 return View();
 }
}
