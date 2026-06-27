namespace SmartHome.Server.Services.Processors;

/// <summary>
/// Definition of processor that execute business logic within a background service.
/// </summary>
public interface IBackgroundServiceProcessor
{
    public string ProcessorName { get; }

    /// <summary>
    /// Performs a single iteration of the background processing logic asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, 
    /// ensuring the task stops gracefully when the host shuts down.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task ProcessAsync(CancellationToken cancellationToken);
}
