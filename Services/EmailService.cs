using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using O_market.Models;
using System.Net;

namespace O_market.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly bool _disableEmailSending;
        private readonly EmailSettings _emailSettings;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
            _disableEmailSending = config["DisableEmailSending"]?.ToLower() == "true";

            _emailSettings = new EmailSettings
            {
                SmtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                SmtpPort = int.TryParse(_config["EmailSettings:SmtpPort"], out int port) ? port : 587,
                SenderEmail = _config["EmailSettings:SenderEmail"] ?? "noreply@omarket.com",
                SenderName = _config["EmailSettings:SenderName"] ?? "QuickSwap",
                Username = _config["EmailSettings:Username"] ?? _config["EmailSettings:SenderEmail"] ?? "",
                Password = _config["EmailSettings:Password"] ?? "",
                EnableSsl = bool.TryParse(_config["EmailSettings:EnableSsl"], out bool ssl) ? ssl : true
            };
        }

        public async Task<(bool Success, string Message)> SendOtpAsync(string email, string otp)
        {
            _logger.LogInformation("MailKit: Attempting to send OTP email to {Email}. DisableEmailSending is {Disabled}", email, _disableEmailSending);

            if (_disableEmailSending)
            {
                _logger.LogInformation("MailKit: [TEST MODE] OTP for {Email}: {Otp}", email, otp);
                return (true, "OTP logged to console (Test Mode)");
            }

            if (string.IsNullOrEmpty(_emailSettings.Username) || string.IsNullOrEmpty(_emailSettings.Password))
            {
                _logger.LogError("MailKit: Email configuration missing: Username or Password is not set.");
                return (false, "Email configuration error: Missing credentials.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "🔐 OTP Verification - QuickSwap";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #4a148c; text-align: center;'>Verify Your Email</h2>
                    <p>Hello,</p>
                    <p>Your OTP for <strong>QuickSwap</strong> is:</p>
                    <div style='background: #f3e5f5; color: #4a148c; font-size: 32px; font-weight: bold; text-align: center; padding: 20px; border-radius: 8px; margin: 20px 0; letter-spacing: 5px;'>
                        {otp}
                    </div>
                    <p>This code expires in 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee;'>
                    <p style='font-size: 12px; color: #888;'>This is an automated message from QuickSwap.</p>
                </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                _logger.LogInformation("MailKit: Connecting to {Server}:{Port}...", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                
                // For Gmail, StartTls is typical for port 587
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);

                _logger.LogInformation("MailKit: Authenticating as {Username}...", _emailSettings.Username);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("MailKit: ✅ OTP email sent successfully to {Email}", email);
                return (true, "OTP sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailKit: ❌ Failed to send email to {Email}. Error: {Message}", email, ex.Message);
                return (false, $"Email failed: {ex.Message}");
            }
        }
    }
}
