namespace HomeControllerHUB.Domain.Interfaces;

/// <summary>
/// Interface for providing date and time functionality.
/// This abstraction is useful for testing and ensuring consistent time handling.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime Now { get; }
    
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Gets today's date (with time component set to midnight).
    /// </summary>
    DateTime Today { get; }
} 