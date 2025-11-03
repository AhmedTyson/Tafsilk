using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using TafsilkPlatform.Web.Services;

namespace TafsilkPlatform.Tests.Services;

public class RateLimitServiceTests
{
    private readonly Mock<ILogger<RateLimitService>> _loggerMock;
    private readonly IMemoryCache _cache;
    private readonly RateLimitService _service;

    public RateLimitServiceTests()
  {
   _loggerMock = new Mock<ILogger<RateLimitService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
 _service = new RateLimitService(_cache, _loggerMock.Object);
    }

    [Fact]
    public async Task IsRateLimitedAsync_NewKey_ReturnsFalse()
    {
        // Arrange
 var key = "test_user@example.com";

        // Act
  var result = await _service.IsRateLimitedAsync(key);

 // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IncrementAsync_FiveAttempts_CausesLockout()
    {
        // Arrange
     var key = "test_user@example.com";

 // Act
        for (int i = 0; i < 5; i++)
     {
            await _service.IncrementAsync(key);
   }

        var isLocked = await _service.IsRateLimitedAsync(key);

        // Assert
        Assert.True(isLocked);
    }

    [Fact]
    public async Task ResetAsync_ClearsLockout()
    {
        // Arrange
   var key = "test_user@example.com";
        
        // Lock the account
    for (int i = 0; i < 5; i++)
        {
  await _service.IncrementAsync(key);
        }

        // Act
 await _service.ResetAsync(key);
        var isLocked = await _service.IsRateLimitedAsync(key);

   // Assert
        Assert.False(isLocked);
    }
}

public class InputSanitizerTests
{
    private readonly InputSanitizer _sanitizer;

    public InputSanitizerTests()
    {
     _sanitizer = new InputSanitizer();
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>Hello", "Hello")]
    [InlineData("<b>Bold</b> text", "Bold text")]
    [InlineData("Normal text", "Normal text")]
    public void SanitizeHtml_RemovesHtmlTags(string input, string expected)
    {
   // Act
        var result = _sanitizer.SanitizeHtml(input);

   // Assert
     Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("01012345678", true)]
    [InlineData("01112345678", true)]
    [InlineData("01212345678", true)]
    [InlineData("01512345678", true)]
    [InlineData("0101234567", false)]  // Too short
    [InlineData("01312345678", false)] // Invalid prefix
    [InlineData("12345678901", false)] // Doesn't start with 01
    public void IsValidPhoneNumber_ValidatesEgyptianNumbers(string phone, bool expected)
    {
        // Act
        var result = _sanitizer.IsValidPhoneNumber(phone);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
 [InlineData("SELECT * FROM Users", true)]
    [InlineData("DROP TABLE Users", true)]
    [InlineData("<script>alert('xss')</script>", true)]
    [InlineData("Normal text", false)]
    [InlineData("أحمد محمد", false)]
    public void ContainsSuspiciousContent_DetectsMaliciousInput(string input, bool expected)
    {
     // Act
        var result = _sanitizer.ContainsSuspiciousContent(input);

   // Assert
  Assert.Equal(expected, result);
    }
}
