using SmartHome.Server.Services.Processors;

namespace SmartHome.Server.Services;

/// <summary>
/// <summary>
/// Implementation of <see cref="BackgroundService"/> that acts as a runner 
/// for an <see cref="IBackgroundServiceProcessor"/>, executing its logic at a specified time interval.
/// </summary>
public sealed class BackgroundProcessorService : BackgroundService
{
    #region Properties
    private readonly IBackgroundServiceProcessor _processor;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _serviceExecutionInterval;
    private readonly ILogger<BackgroundProcessorService> _logger;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="BackgroundProcessorService"/>.
    /// </summary>
    /// <param name="serviceProcessor">
    /// Processor of background service which shall be executed by created instance.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by created instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value of at least one argument is outside its valid range.
    /// </exception>
    public BackgroundProcessorService(
        IBackgroundServiceProcessor serviceProcessor,
        TimeProvider timeProvider,
        TimeSpan serviceExecutionInterval,
        ILogger<BackgroundProcessorService> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProcessor, nameof(serviceProcessor));
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(serviceExecutionInterval, TimeSpan.Zero);

        _processor = serviceProcessor;
        _timeProvider = timeProvider;
        _serviceExecutionInterval = serviceExecutionInterval;
        _logger = logger;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Executes the logic defined in the wrapped service processor within a background 
    /// processing loop until the application host shuts down.
    /// </summary>
    /// <param name="cancellationToken">
    /// Triggered when the application host is performing a graceful shutdown.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the long-running background operation.
    /// </returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background service execution started: ProcessorName=[{ProcessorName}]", _processor.ProcessorName);

        using var serviceIntervalTimer = new PeriodicTimer(_serviceExecutionInterval, _timeProvider);

        try
        {
            while (await serviceIntervalTimer.WaitForNextTickAsync(cancellationToken))
            {
                await _processor.ProcessAsync(cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception thrown in service execution loop:");
        }

        _logger.LogInformation("Background service execution stopped: ProcessorName=[{ProcessorName}]", _processor.ProcessorName);
    }
    #endregion
}
