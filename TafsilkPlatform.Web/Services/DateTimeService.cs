using System;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Service for managing Egypt timezone (Cairo) throughout the application
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current date and time in Egypt timezone (Cairo)
    /// </summary>
 DateTime Now { get; }

    /// <summary>
  /// Gets the current date in Egypt timezone (Cairo)
    /// </summary>
    DateTime Today { get; }

    /// <summary>
    /// Converts UTC time to Egypt timezone
    /// </summary>
    DateTime ConvertFromUtc(DateTime utcDateTime);

    /// <summary>
    /// Converts Egypt time to UTC
    /// </summary>
    DateTime ConvertToUtc(DateTime egyptDateTime);

    /// <summary>
    /// Gets Egypt TimeZoneInfo
    /// </summary>
    TimeZoneInfo EgyptTimeZone { get; }
}

public class DateTimeService : IDateTimeService
{
    private readonly TimeZoneInfo _egyptTimeZone;

    public DateTimeService()
    {
        // Egypt Standard Time (Cairo)
        // UTC+2 (Standard) / UTC+3 (Daylight Saving Time when applicable)
        try
        {
     _egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
   }
    catch
        {
     // Fallback for Linux/Mac systems
    try
  {
        _egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
   }
            catch
   {
   // Final fallback: Create custom timezone
          _egyptTimeZone = TimeZoneInfo.CreateCustomTimeZone(
          "Egypt Standard Time",
         TimeSpan.FromHours(2),
"Egypt Standard Time",
   "Egypt Standard Time");
            }
        }
    }

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _egyptTimeZone);

    public DateTime Today => Now.Date;

  public DateTime ConvertFromUtc(DateTime utcDateTime)
    {
 if (utcDateTime.Kind != DateTimeKind.Utc)
        {
        utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
    }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _egyptTimeZone);
    }

    public DateTime ConvertToUtc(DateTime egyptDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(egyptDateTime, _egyptTimeZone);
    }

    public TimeZoneInfo EgyptTimeZone => _egyptTimeZone;
}
