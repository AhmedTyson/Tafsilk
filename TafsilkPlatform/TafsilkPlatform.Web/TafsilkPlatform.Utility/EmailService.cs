using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;

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
/// Service for sending emails using SendGrid or SMTP (Fallback)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    // SMTP Settings
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;

    // SendGrid Settings
    private readonly string _sendGridApiKey;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Load Email Settings
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@tafsilk.com";
        _fromName = _configuration["Email:FromName"] ?? "Tafsilk Platform";

        // SMTP
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _username = _configuration["Email:Username"] ?? "";
        _password = _configuration["Email:Password"] ?? "";
        _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

        // SendGrid
        _sendGridApiKey = _configuration["Email:SendGridApiKey"] ?? "";
    }

    /// <summary>
    /// Send email verification link
    /// </summary>
    public async Task<bool> SendEmailVerificationAsync(string email, string fullName, string verificationToken)
    {
        try
        {
            var verificationUrl = $"{_configuration["App:BaseUrl"]}/Account/VerifyEmail?token={verificationToken}&email={WebUtility.UrlEncode(email)}";

            var subject = "Email Verification - Tafsilk Platform";
            var body = $@"
<!DOCTYPE html>
<html dir='ltr' lang='en'>
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
            <div class='logo'>🔧 Tafsilk</div>
        </div>
        <div class='title'>Hello {fullName},</div>
        <div class='message'>
            <p>Thank you for registering with Tafsilk Platform.</p>
            <p>To verify your email address, please click the button below:</p>
        </div>
        <div style='text-align: center;'>
            <a href='{verificationUrl}' class='button'>Verify Email</a>
        </div>
        <div class='message' style='margin-top: 30px;'>
            <p><strong>Or copy the following link into your browser:</strong></p>
            <p style='word-break: break-all; color: #2563eb;'>{verificationUrl}</p>
        </div>
        <div class='footer'>
            <p>© 2024 Tafsilk Platform - All rights reserved</p>
            <p>This email was sent automatically, please do not reply</p>
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

            var subject = "Password Reset - Tafsilk Platform";
            var body = $@"
<!DOCTYPE html>
<html dir='ltr' lang='en'>
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
            <div class='logo'>🔧 Tafsilk</div>
        </div>
        <div class='title'>Hello {fullName},</div>
        <div class='message'>
            <p>We received a request to reset your password.</p>
            <p>To reset your password, please click the button below:</p>
        </div>
        <div style='text-align: center;'>
            <a href='{resetUrl}' class='button'>Reset Password</a>
        </div>
        <div class='message' style='margin-top: 30px;'>
            <p><strong>Or copy the following link into your browser:</strong></p>
            <p style='word-break: break-all; color: #2563eb;'>{resetUrl}</p>
        </div>
        <div class='message'>
            <p style='color: #ef4444; font-weight: 600;'>⚠️ This link is valid for one hour only</p>
            <p style='font-size: 14px; color: #6b7280;'>If you did not request a password reset, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© 2024 Tafsilk Platform - All rights reserved</p>
            <p>This email was sent automatically, please do not reply</p>
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
                "Customer" => "Customer",
                "Tailor" => "Tailor",
                "Admin" => "Admin",
                _ => "User"
            };

            var subject = "Welcome to Tafsilk Platform!";
            var body = $@"
<!DOCTYPE html>
<html dir='ltr' lang='en'>
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
            <div class='logo'>🎉 Tafsilk</div>
        </div>
        <div class='title'>Hello {fullName},</div>
        <div class='message'>
            <p>Welcome to Tafsilk Platform as a <strong>{roleText}</strong>!</p>
            <p>Your email has been successfully verified and you can now enjoy all the platform features.</p>
        </div>
        <div style='text-align: center;'>
            <a href='{_configuration["App:BaseUrl"]}' class='button'>Start Now</a>
        </div>
        <div class='footer'>
            <p>© 2024 Tafsilk Platform - All rights reserved</p>
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
<html dir='ltr' lang='en'>
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
            <div class='logo'>🔧 Tafsilk</div>
        </div>
        <div class='message'>
            {message}
        </div>
        <div class='footer'>
            <p>© 2024 Tafsilk Platform - All rights reserved</p>
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
    /// Core email sending method (SendGrid with SMTP Fallback)
    /// </summary>
    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        // 1. Try SendGrid if API Key is present
        if (!string.IsNullOrEmpty(_sendGridApiKey))
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully via SendGrid to {Email}", toEmail);
                    return true;
                }
                else
                {
                    _logger.LogWarning("SendGrid failed with status code {StatusCode}. Falling back to SMTP.", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via SendGrid to {Email}. Falling back to SMTP.", toEmail);
            }
        }

        // 2. Fallback to SMTP
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            _logger.LogWarning("SMTP not configured. Skipping email to {Email}", toEmail);
            _logger.LogInformation("EMAIL PREVIEW:\nTo: {Email}\nSubject: {Subject}\nBody: {Body}",
                toEmail, subject, htmlBody[..Math.Min(200, htmlBody.Length)]);
            return true; // Simulate success in dev
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

            _logger.LogInformation("Email sent successfully via SMTP to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Email}", toEmail);
            return false;
        }
    }
}
