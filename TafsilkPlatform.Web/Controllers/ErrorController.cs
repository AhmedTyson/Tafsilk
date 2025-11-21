using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Error handling controller for custom error pages
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
public class ErrorController : Controller
{
    [HttpGet]
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            500 => View("InternalServerError"),
            _ => View("Error", new ErrorViewModel { StatusCode = statusCode })
        };
    }

    [HttpGet]
    [Route("Error")]
    public IActionResult Error()
    {
        return View("Error");
    }
}

