using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Security;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Web.Controllers
{
 [ApiController]
 [Route("api/auth")]
 public class AuthApiController : ControllerBase
 {
 private readonly IUnitOfWork _uow;
 private readonly ITokenService _tokenService;

 public AuthApiController(IUnitOfWork uow, ITokenService tokenService)
 {
 _uow = uow;
 _tokenService = tokenService;
 }

 // POST api/auth/token
 [HttpPost("token")]
 [AllowAnonymous]
 public async Task<IActionResult> CreateToken([FromBody] TokenRequest req)
 {
 var user = await _uow.Context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
 if (user == null) return Unauthorized();
 // verify password
 if (!TafsilkPlatform.Web.Security.PasswordHasher.Verify(user.PasswordHash, req.Password)) return Unauthorized();
 // create token
 var role = user.Role?.Name ?? string.Empty;
 var jwt = _tokenService.CreateToken(user.Id, user.Email, role, TimeSpan.FromHours(4));
 return Ok(new { token = jwt });
 }

 public record TokenRequest(string Email, string Password);
 }
}
