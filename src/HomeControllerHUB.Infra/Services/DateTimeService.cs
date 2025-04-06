using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Infra.Services;

/// <summary>
/// Implementation of IDateTime that provides the actual system time.
/// </summary>
public class DateTimeService : IDateTime
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    public DateTime Now => DateTime.Now;
    
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
    
    /// <summary>
    /// Gets today's date (with time component set to midnight).
    /// </summary>
    public DateTime Today => DateTime.Today;
} 