namespace SmartHome.Server.Data;

/// <summary>
/// Provides methods for obtaining timestamps.
/// </summary>
public interface ITimestampProvider
{
    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    /// <returns>
    /// The current UTC timestamp.
    /// </returns>
    DateTime GetUtcNow();
}

/// <inheritdoc cref="ITimestampProvider"/>
public sealed class TimestampProvider : ITimestampProvider
{
    /// <inheritdoc cref="ITimestampProvider"/>
    public DateTime GetUtcNow() =>
        DateTime.UtcNow;
}
