namespace TafsilkPlatform.Shared.Extensions
{
    /// <summary>
 /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
 /// <summary>
     /// Check if string is null or whitespace
        /// </summary>
   public static bool IsNullOrEmpty(this string? value)
        {
   return string.IsNullOrWhiteSpace(value);
   }

      /// <summary>
  /// Truncate string to specified length
   /// </summary>
   public static string Truncate(this string value, int maxLength)
   {
   if (string.IsNullOrWhiteSpace(value))
     return string.Empty;

 return value.Length <= maxLength ? value : value[..maxLength] + "...";
 }

        /// <summary>
        /// Convert to title case (Arabic-aware)
     /// </summary>
      public static string ToTitleCase(this string value)
   {
if (string.IsNullOrWhiteSpace(value))
   return string.Empty;

    var textInfo = new System.Globalization.CultureInfo("ar-EG", false).TextInfo;
      return textInfo.ToTitleCase(value.ToLower());
        }
 }

 /// <summary>
    /// Extension methods for DateTime
/// </summary>
    public static class DateTimeExtensions
  {
        /// <summary>
     /// Check if date is today
/// </summary>
        public static bool IsToday(this DateTime date)
   {
 return date.Date == DateTime.Today;
     }

    /// <summary>
  /// Check if date is in the past
        /// </summary>
        public static bool IsPast(this DateTime date)
   {
   return date < DateTime.Now;
        }

/// <summary>
 /// Get friendly time ago string (Arabic)
   /// </summary>
        public static string ToFriendlyString(this DateTime date)
 {
      var timeSpan = DateTime.Now - date;

   if (timeSpan.TotalMinutes < 1)
      return "الآن";
            if (timeSpan.TotalMinutes < 60)
 return $"منذ {(int)timeSpan.TotalMinutes} دقيقة";
   if (timeSpan.TotalHours < 24)
   return $"منذ {(int)timeSpan.TotalHours} ساعة";
    if (timeSpan.TotalDays < 7)
      return $"منذ {(int)timeSpan.TotalDays} يوم";
 if (timeSpan.TotalDays < 30)
    return $"منذ {(int)(timeSpan.TotalDays / 7)} أسبوع";
    if (timeSpan.TotalDays < 365)
     return $"منذ {(int)(timeSpan.TotalDays / 30)} شهر";

   return $"منذ {(int)(timeSpan.TotalDays / 365)} سنة";
        }
    }

  /// <summary>
    /// Extension methods for decimal (pricing)
 /// </summary>
    public static class DecimalExtensions
  {
        /// <summary>
     /// Format as Egyptian currency
   /// </summary>
    public static string ToEgyptianCurrency(this decimal amount)
{
   return $"{amount:N0} جنيه";
  }

   /// <summary>
/// Format as currency with decimal places
  /// </summary>
        public static string ToEgyptianCurrencyDetailed(this decimal amount)
     {
   return $"{amount:N2} جنيه";
 }
    }

    /// <summary>
 /// Extension methods for lists
  /// </summary>
    public static class ListExtensions
{
        /// <summary>
        /// Check if list is null or empty
   /// </summary>
   public static bool IsNullOrEmpty<T>(this List<T>? list)
     {
   return list == null || list.Count == 0;
  }

  /// <summary>
   /// Get random item from list
        /// </summary>
      public static T? GetRandom<T>(this List<T> list)
   {
            if (list.IsNullOrEmpty())
    return default;

var random = new Random();
   return list[random.Next(list.Count)];
        }

   /// <summary>
        /// Paginate list
        /// </summary>
  public static List<T> Paginate<T>(this List<T> list, int page, int pageSize)
   {
   if (list.IsNullOrEmpty())
    return new List<T>();

       return list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
   }
    }
}
