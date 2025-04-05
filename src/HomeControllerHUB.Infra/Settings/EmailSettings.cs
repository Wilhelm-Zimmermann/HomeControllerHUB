namespace HomeControllerHUB.Infra.Settings;

/// <summary>
/// Email settings configuration class
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Mailgun API key
    /// </summary>
    public string MailgunApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Mailgun domain
    /// </summary>
    public string MailgunDomain { get; set; } = string.Empty;
    
    /// <summary>
    /// Frontend URL for email links
    /// </summary>
    public string FrontendUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Sender email address
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Sender display name
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
} 