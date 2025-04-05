using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.Settings;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Infra.Services;

/// <summary>
/// Implementation of the email service using Mailgun
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly HttpClient _httpClient;
    private readonly string _mailgunApiKey;
    private readonly string _mailgunDomain;
    private readonly string _frontendUrl;

    /// <summary>
    /// Constructor for EmailService
    /// </summary>
    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _httpClient = httpClientFactory.CreateClient("Mailgun");
        _mailgunApiKey = _emailSettings.MailgunApiKey;
        _mailgunDomain = _emailSettings.MailgunDomain;
        _frontendUrl = _emailSettings.FrontendUrl;
    }

    /// <inheritdoc />
    public async Task SendEmailConfirmationAsync(string email, string name, string token)
    {
        try
        {
            // Create confirmation URL with token and email
            var confirmationUrl = $"{_frontendUrl}/confirm-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            
            var subject = "Confirm your email address";
            var body = GetEmailConfirmationTemplate(name, confirmationUrl);
            
            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendPasswordResetAsync(string email, string name, string token)
    {
        try
        {
            // Create reset URL with token and email
            var resetUrl = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            
            var subject = "Reset your password";
            var body = GetPasswordResetTemplate(name, resetUrl);
            
            await SendEmailAsync(email, subject, body);
            
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            #if DEBUG
            // In debug mode, just log the email instead of sending it
            _logger.LogInformation("Email would be sent to {To} with subject '{Subject}' and body: {Body}", to, subject, body);
            return;
            #endif

            // Prepare basic authentication for Mailgun API
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_mailgunApiKey}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            // Prepare form content for Mailgun API
            var formContent = new MultipartFormDataContent
            {
                { new StringContent(_emailSettings.SenderEmail), "from" },
                { new StringContent(to), "to" },
                { new StringContent(subject), "subject" },
                { new StringContent(body), isHtml ? "html" : "text" }
            };

            // Send the request to Mailgun API
            var response = await _httpClient.PostAsync($"https://api.mailgun.net/v3/{_mailgunDomain}/messages", formContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Mailgun API returned error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                throw new Exception($"Failed to send email. Mailgun API returned {response.StatusCode}");
            }

            _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
            throw;
        }
    }

    private string GetEmailConfirmationTemplate(string name, string confirmationUrl)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Confirm Your Email</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {name},</p>
                        <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                        <p style='text-align: center;'>
                            <a href='{confirmationUrl}' class='button'>Confirm Email</a>
                        </p>
                        <p>If the button doesn't work, you can also copy and paste the following link into your browser:</p>
                        <p>{confirmationUrl}</p>
                        <p>This link will expire in 24 hours.</p>
                        <p>If you didn't register for an account, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message. Please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GetPasswordResetTemplate(string name, string resetUrl)
    {
        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #2196F3; color: white; padding: 10px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .button {{ background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Reset Your Password</h1>
                    </div>
                    <div class='content'>
                        <p>Hello {name},</p>
                        <p>We received a request to reset your password. Click the button below to create a new password:</p>
                        <p style='text-align: center;'>
                            <a href='{resetUrl}' class='button'>Reset Password</a>
                        </p>
                        <p>If the button doesn't work, you can also copy and paste the following link into your browser:</p>
                        <p>{resetUrl}</p>
                        <p>This link will expire in 24 hours.</p>
                        <p>If you didn't request a password reset, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message. Please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
} 