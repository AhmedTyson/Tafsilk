using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TafsilkPlatform.Web.Security
{
 public interface ITokenService
 {
 string CreateToken(Guid userId, string email, string role, TimeSpan? lifetime = null);
 }

 public class TokenService : ITokenService
 {
 private readonly IConfiguration _config;
 private readonly byte[] _key;
 private readonly string _issuer;
 private readonly string _audience;

 public TokenService(IConfiguration config)
 {
 _config = config;
 var keyString = _config["Jwt:Key"] ?? "ReplaceThisWithASecretKeyInProduction";
 _key = Encoding.UTF8.GetBytes(keyString);
 _issuer = _config["Jwt:Issuer"] ?? "tafsilk";
 _audience = _config["Jwt:Audience"] ?? "tafsilk_clients";
 }

 public string CreateToken(Guid userId, string email, string role, TimeSpan? lifetime = null)
 {
 var claims = new[] {
 new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
 new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty),
 new Claim(ClaimTypes.Role, role ?? string.Empty),
 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
 };

 var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);
 var expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1));

 var token = new JwtSecurityToken(
 issuer: _issuer,
 audience: _audience,
 claims: claims,
 expires: expires,
 signingCredentials: creds
 );

 return new JwtSecurityTokenHandler().WriteToken(token);
 }
 }
}
