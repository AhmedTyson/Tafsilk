using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TafsilkPlatform.Utility;

/// <summary>
/// Interface for email service
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailVerificationAsync(string email, string fullName, string verificationToken);
    Task<bool> SendPasswordResetAsync(string email, string fullName, string resetToken);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName, string role);
    Task<bool> SendNotificationAsync(string email, string subject, string message);
}

/// <summary>
/// Service for sending emails using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;

    public EmailService(
        IConfiguration configuration,
     ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Load SMTP settings from configuration
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@tafsilk.com";
        _fromName = _configuration["Email:FromName"] ?? "منصة تفصيلك";
        _username = _configuration["Email:Username"] ?? "";
        _password = _configuration["Email:Password"] ?? "";
        _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

        // ✅ FIXED: Only log email configuration warning once at startup (not on every request)
        // This reduces log noise - the warning is logged in Program.cs during startup
    }

    /// <summary>
    /// Send email verification link
    /// </summary>
    public async Task<bool> SendEmailVerificationAsync(string email, string fullName, string verificationToken)
    {
        try
        {
            var verificationUrl = $"{_configuration["App:BaseUrl"]}/Account/VerifyEmail?token={verificationToken}";

            var subject = "تأكيد البريد الإلكتروني - منصة تفصيلك";
            var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='utf-8'>
    <style>
     body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f3f4f6; padding: 20px; }}
 .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
   .header {{ text-align: center; margin-bottom: 30px; }}
     .logo {{ font-size: 32px; font-weight: 800; color: #2563eb; }}
   .title {{ font-size: 24px; font-weight: 700; color: #1f2937; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #374151; line-height: 1.6; margin-bottom: 30px; }}
        .button {{ display: inline-block; background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; }}
    .footer {{ margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 14px; color: #6b7280; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
      <div class='logo'>🔧 تفصيلك</div>
        </div>
    <div class='title'>مرحباً {fullName}،</div>
        <div class='message'>
            <p>شكراً لتسجيلك في منصة تفصيلك!</p>
          <p>لإكمال عملية التسجيل، يرجى تأكيد بريدك الإلكتروني بالضغط على الزر أدناه:</p>
        </div>
     <div style='text-align: center;'>
            <a href='{verificationUrl}' class='button'>تأكيد البريد الإلكتروني</a>
        </div>
     <div class='message' style='margin-top: 30px;'>
            <p><strong>أو انسخ الرابط التالي في متصفحك:</strong></p>
          <p style='word-break: break-all; color: #2563eb;'>{verificationUrl}</p>
    </div>
  <div class='message'>
   <p style='color: #ef4444; font-weight: 600;'>⚠️ هذا الرابط صالح لمدة 24 ساعة فقط</p>
            <p style='font-size: 14px; color: #6b7280;'>إذا لم تقم بإنشاء هذا الحساب، يمكنك تجاهل هذا البريد.</p>
        </div>
        <div class='footer'>
            <p>© 2024 منصة تفصيلك - جميع الحقوق محفوظة</p>
 <p>هذا البريد تم إرساله تلقائياً، يرجى عدم الرد عليه</p>
    </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Send password reset link
    /// </summary>
    public async Task<bool> SendPasswordResetAsync(string email, string fullName, string resetToken)
    {
        try
        {
            var resetUrl = $"{_configuration["App:BaseUrl"]}/Account/ResetPassword?token={resetToken}";

            var subject = "إعادة تعيين كلمة المرور - منصة تفصيلك";
            var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
<meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f3f4f6; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
  .header {{ text-align: center; margin-bottom: 30px; }}
    .logo {{ font-size: 32px; font-weight: 800; color: #2563eb; }}
        .title {{ font-size: 24px; font-weight: 700; color: #1f2937; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #374151; line-height: 1.6; margin-bottom: 30px; }}
     .button {{ display: inline-block; background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; }}
        .footer {{ margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 14px; color: #6b7280; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
         <div class='logo'>🔧 تفصيلك</div>
    </div>
    <div class='title'>مرحباً {fullName}،</div>
        <div class='message'>
      <p>تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بحسابك.</p>
    <p>لإعادة تعيين كلمة المرور، يرجى الضغط على الزر أدناه:</p>
        </div>
        <div style='text-align: center;'>
       <a href='{resetUrl}' class='button'>إعادة تعيين كلمة المرور</a>
        </div>
 <div class='message' style='margin-top: 30px;'>
            <p><strong>أو انسخ الرابط التالي في متصفحك:</strong></p>
  <p style='word-break: break-all; color: #2563eb;'>{resetUrl}</p>
  </div>
        <div class='message'>
 <p style='color: #ef4444; font-weight: 600;'>⚠️ هذا الرابط صالح لمدة ساعة واحدة فقط</p>
  <p style='font-size: 14px; color: #6b7280;'>إذا لم تطلب إعادة تعيين كلمة المرور، يمكنك تجاهل هذا البريد بأمان.</p>
        </div>
<div class='footer'>
        <p>© 2024 منصة تفصيلك - جميع الحقوق محفوظة</p>
            <p>هذا البريد تم إرساله تلقائياً، يرجى عدم الرد عليه</p>
 </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Send welcome email after registration
    /// </summary>
    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName, string role)
    {
        try
        {
            var roleText = role switch
            {
                "Customer" => "عميل",
                "Tailor" => "خياط",
                // "Corporate" => "عميل مؤسسي", // REMOVED: Corporate feature
                "Admin" => "مدير",
                _ => "مستخدم"
            };

            var subject = "مرحباً بك في منصة تفصيلك!";
            var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f3f4f6; padding: 20px; }}
   .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 32px; font-weight: 800; color: #2563eb; }}
        .title {{ font-size: 24px; font-weight: 700; color: #1f2937; margin-bottom: 20px; }}
        .message {{ font-size: 16px; color: #374151; line-height: 1.6; margin-bottom: 30px; }}
        .button {{ display: inline-block; background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; }}
   .footer {{ margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 14px; color: #6b7280; }}
    </style>
</head>
<body>
    <div class='container'>
      <div class='header'>
 <div class='logo'>🎉 تفصيلك</div>
        </div>
        <div class='title'>مرحباً {fullName}،</div>
        <div class='message'>
 <p>نرحب بك في منصة تفصيلك كـ <strong>{roleText}</strong>!</p>
          <p>تم تأكيد بريدك الإلكتروني بنجاح ويمكنك الآن الاستمتاع بجميع مزايا المنصة.</p>
        </div>
        <div style='text-align: center;'>
     <a href='{_configuration["App:BaseUrl"]}' class='button'>ابدأ الآن</a>
        </div>
        <div class='footer'>
      <p>© 2024 منصة تفصيلك - جميع الحقوق محفوظة</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Send generic notification email
    /// </summary>
    public async Task<bool> SendNotificationAsync(string email, string subject, string message)
    {
        try
        {
            var body = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f3f4f6; padding: 20px; }}
     .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 32px; font-weight: 800; color: #2563eb; }}
 .message {{ font-size: 16px; color: #374151; line-height: 1.6; margin-bottom: 30px; }}
 .footer {{ margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 14px; color: #6b7280; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
    <div class='logo'>🔧 تفصيلك</div>
        </div>
        <div class='message'>
            {message}
        </div>
     <div class='footer'>
            <p>© 2024 منصة تفصيلك - جميع الحقوق محفوظة</p>
        </div>
  </div>
</body>
</html>";

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Core email sending method
    /// </summary>
    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        // Check if email is configured
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            _logger.LogWarning("Email service not configured. Skipping email to {Email}", toEmail);
            // In development, just log the email instead of sending
            _logger.LogInformation("EMAIL PREVIEW:\nTo: {Email}\nSubject: {Subject}\nBody: {Body}",
      toEmail, subject, htmlBody.Substring(0, Math.Min(200, htmlBody.Length)));
            return true; // Return true in development mode
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_fromEmail, _fromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.Normal;

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
            smtpClient.EnableSsl = _enableSsl;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_username, _password);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Timeout = 30000; // 30 seconds

            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Email}: {Error}", toEmail, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Email}", toEmail);
            return false;
        }
    }
}
