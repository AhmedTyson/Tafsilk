namespace TafsilkPlatform.Shared.Constants
{
    /// <summary>
    /// Shared application constants
    /// </summary>
    public static class AppConstants
    {
    // User Roles
  public static class Roles
    {
   public const string Customer = "Customer";
    public const string Tailor = "Tailor";
      public const string Admin = "Admin";
        }

        // Order Status
    public static class OrderStatus
    {
     public const string New = "جديد";
     public const string Pending = "قيد الانتظار";
   public const string InProgress = "قيد التنفيذ";
         public const string Completed = "مكتمل";
        public const string Cancelled = "ملغي";
        }

      // Cities (Egyptian cities) - Direct list access
        public static readonly List<string> Cities = new()
        {
       "القاهرة",
    "الإسكندرية",
      "الجيزة",
         "الأقصر",
             "أسوان",
    "بورسعيد",
         "السويس",
         "المنصورة",
           "طنطا",
    "أسيوط",
              "الفيوم",
     "الزقازيق",
             "دمياط",
                "الإسماعيلية"
     };

        // Specialties - Direct list access
        public static readonly List<string> Specialties = new()
      {
      "بدلات رجالية",
   "فساتين سهرة",
                "عبايات فاخرة",
      "فساتين زفاف",
    "قمصان مخصصة",
                "جلابيات",
          "ملابس كاجوال",
             "ملابس رياضية",
          "ملابس أطفال",
    "تطريز وتزيين"
   };

      // Service Categories
public static class ServiceCategories
        {
     public static readonly List<string> Categories = new()
     {
                "ملابس رجالية",
  "ملابس نسائية",
        "عبايات",
             "فساتين",
    "بدلات",
      "قمصان",
    "جلابيات",
       "ملابس أطفال"
            };
        }

      // Validation
   public static class Validation
        {
            public const int MinPasswordLength = 6;
     public const int MaxPasswordLength = 100;
      public const int MinNameLength = 3;
            public const int MaxNameLength = 100;
    public const string EgyptianPhoneRegex = @"^01[0-2,5]\d{8}$";
   }

     // Pricing
        public static class Pricing
        {
   public const decimal MinServicePrice = 1m;
     public const decimal MaxServicePrice = 100000m;
          public const int MinEstimatedDays = 1;
            public const int MaxEstimatedDays = 365;
        }

        // Error Messages (Arabic)
        public static class ErrorMessages
   {
       public const string ProfileNotFound = "الملف الشخصي غير موجود";
            public const string Unauthorized = "غير مصرح بهذا الإجراء";
            public const string GeneralError = "حدث خطأ. يرجى المحاولة مرة أخرى";
            public const string InvalidCredentials = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
            public const string EmailExists = "البريد الإلكتروني مستخدم بالفعل";
         public const string ServiceNotFound = "الخدمة غير موجودة";
            public const string OrderNotFound = "الطلب غير موجود";
            public const string AddressNotFound = "العنوان غير موجود";
}

    // Success Messages (Arabic)
  public static class SuccessMessages
        {
   public const string ProfileUpdated = "تم تحديث الملف الشخصي بنجاح";
       public const string ServiceAdded = "تمت إضافة الخدمة بنجاح";
    public const string ServiceUpdated = "تم تحديث الخدمة بنجاح";
public const string ServiceDeleted = "تم حذف الخدمة بنجاح";
     public const string AddressAdded = "تمت إضافة العنوان بنجاح";
            public const string AddressUpdated = "تم تحديث العنوان بنجاح";
       public const string AddressDeleted = "تم حذف العنوان بنجاح";
  public const string OrderCreated = "تم إنشاء الطلب بنجاح";
        }

        // Configuration
        public static class Configuration
        {
 public const int DefaultPageSize = 10;
            public const int MaxPageSize = 100;
            public const int SessionTimeoutMinutes = 30;
  public const int CookieExpirationDays = 30;
 }
    }
}
