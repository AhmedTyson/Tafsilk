using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace TafsilkPlatform.Web.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailMessage = new MimeMessage();
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@tafsilk.com";
                var fromName = _configuration["EmailSettings:FromName"] ?? "Tafsilk Platform";

                emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                // Accept all SSL certificates (in case of self-signed certificates)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    throw new InvalidOperationException("SMTP configuration is missing. Please check appsettings.json");
                }

                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email to {email} sent successfully via SMTP!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failure sending email to {email}");
                throw;
            }
        }
    }
}
