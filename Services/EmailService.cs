using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using O_market.Models;
using System.Net;
using System.Net.Mail;

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
            _logger.LogInformation("Attempting to send OTP email to {Email}. DisableEmailSending is {Disabled}", email, _disableEmailSending);

            if (_disableEmailSending)
            {
                _logger.LogInformation("[TEST MODE] OTP for {Email}: {Otp}", email, otp);
                Console.WriteLine($"🔐 [TEST MODE] OTP for {email}: {otp}");
                return (true, "OTP logged to console (Test Mode)");
            }

            if (string.IsNullOrEmpty(_emailSettings.Username) || string.IsNullOrEmpty(_emailSettings.Password))
            {
                _logger.LogError("Email configuration missing: Username or Password is not set.");
                return (false, "Email configuration error: Missing credentials.");
            }

            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "🔐 OTP Verification - QuickSwap",
                    Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                            <h2 style='color: #4a148c;'>Verify Your Email Address</h2>
                            <p>Hello,</p>
                            <p>Use the following OTP to complete your registration on <strong>QuickSwap</strong>:</p>
                            <div style='background: #f3e5f5; color: #4a148c; font-size: 32px; font-weight: bold; letter-spacing: 5px; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                                {otp}
                            </div>
                            <p><strong>⏰ Valid for 10 minutes.</strong></p>
                            <p>If you didn't request this, please ignore this email.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin-top: 30px;'>
                            <p style='color: #777; font-size: 12px;'>The QuickSwap Team</p>
                        </div>
                    </body>
                    </html>",
                    IsBodyHtml = true
                };

                message.To.Add(email);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 15000
                };

                _logger.LogInformation("Sending email via {SmtpServer}:{Port} using user {Username}...", _emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.Username);
                
                await smtpClient.SendMailAsync(message);

                _logger.LogInformation("✅ OTP email sent successfully to {Email}", email);
                Console.WriteLine($"✅ OTP email sent successfully to {email}");
                return (true, "OTP sent successfully");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Error sending to {Email}. Status: {StatusCode}", email, smtpEx.StatusCode);
                Console.WriteLine($"❌ SMTP Error for {email}: {smtpEx.Message}");
                return (false, $"SMTP Error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}", email);
                Console.WriteLine($"❌ Email failed for {email}: {ex.Message}");
                return (false, $"Failed to send email: {ex.Message}");
            }
        }
    }
}
