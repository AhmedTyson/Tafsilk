namespace TafsilkPlatform.Application.Models.Auth
{
 public class TokenResponse
 {
 public string Token { get; set; } = string.Empty;
 public DateTime ExpiresAtUtc { get; set; }
 }
}
