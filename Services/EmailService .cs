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
            _disableEmailSending = config["DisableEmailSending"] == "true";

            _emailSettings = new EmailSettings
            {
                SmtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                SmtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587"),
                SenderEmail = _config["EmailSettings:SenderEmail"] ?? "noreply@omarket.com",
                SenderName = _config["EmailSettings:SenderName"] ?? "O_market",
                Username = _config["EmailSettings:Username"] ?? "",
                Password = _config["EmailSettings:Password"] ?? "",
                EnableSsl = bool.Parse(_config["EmailSettings:EnableSsl"] ?? "true")
            };
        }

        public async Task<(bool Success, string Message)> SendOtpAsync(string email, string otp)
        {
            if (_disableEmailSending)
            {
                _logger.LogInformation("[TEST MODE] OTP for {Email}: {Otp}", email, otp);
                Console.WriteLine($"🔐 OTP for {email}: {otp}");
                return (true, "OTP logged to console (Test Mode)");
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
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }}
                            .otp-box {{ 
                                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                                color: white;
                                font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 8px;
                                padding: 20px;
                                text-align: center;
                                border-radius: 8px;
                                margin: 25px 0;
                                font-family: monospace;
                            }}
                            .footer {{ 
                                margin-top: 30px; 
                                padding-top: 20px; 
                                border-top: 1px solid #eee; 
                                color: #777; 
                                font-size: 12px; 
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h2>Verify Your Email Address</h2>
                            <p>Hello,</p>
                            <p>Use the following OTP to complete your registration on <strong>QuickSwap</strong>:</p>
                            
                            <div class='otp-box'>{otp}</div>
                            
                            <p><strong>⏰ This OTP is valid for 10 minutes.</strong></p>
                            <p>If you didn't request this OTP, please ignore this email.</p>
                            
                            <div class='footer'>
                                <p>Thank you,<br>The QuickSwap Team</p>
                                <p>This is an automated message, please do not reply.</p>
                            </div>
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
                    Timeout = 10000
                };

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation("✅ OTP email sent successfully to {Email}", email);
                return (true, "OTP sent successfully to your email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                Console.WriteLine($"⚠️ Email failed, but OTP for {email}: {otp}");
                return (false, $"Failed to send email: {ex.Message}. OTP has been logged for testing.");
            }
        }
    }
}