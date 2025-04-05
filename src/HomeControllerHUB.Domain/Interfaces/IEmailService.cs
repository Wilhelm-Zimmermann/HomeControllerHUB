using System.Threading.Tasks;

namespace HomeControllerHUB.Domain.Interfaces;

/// <summary>
/// Interface for email service
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email confirmation to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="name">User's name</param>
    /// <param name="token">Confirmation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailConfirmationAsync(string email, string name, string token);

    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="name">User's name</param>
    /// <param name="token">Password reset token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendPasswordResetAsync(string email, string name, string token);

    /// <summary>
    /// Sends a general email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
} 