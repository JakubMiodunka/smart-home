using Microsoft.Extensions.Logging;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Services.Abstractions;

namespace SmartHome.Server.Services.Processors;

/// <summary>
/// A processor responsible for monitoring station heartbeats and marking inactive stations as offline.
/// </summary>
/// <remarks>
/// This class encapsulates the business logic for determining station timeouts. 
/// It is intended to be executed periodically by a <see cref="BackgroundProcessorService"> instance.
/// </remarks>
internal sealed class HeartbeatMonitoringServiceProcessor : IBackgroundServiceProcessor
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _maxHeartbeatInterval;
    private readonly ILogger<HeartbeatMonitoringServiceProcessor> _logger;

    public string ProcessorName =>
        nameof(HeartbeatMonitoringServiceProcessor);
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="HeartbeatMonitoringServiceProcessor"/>.
    /// </summary>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by created instance.
    /// </param>
    /// <param name="timeProvider">
    /// Time reference shall be used by the instance to coordinate time-based operations.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by created instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value of at least one argument is outside its valid range.
    /// </exception>
    public HeartbeatMonitoringServiceProcessor(
        IStationsRepository stationsRepository,
        TimeProvider timeProvider,
        TimeSpan maxHeartbeatInterval,
        ILogger<HeartbeatMonitoringServiceProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxHeartbeatInterval, TimeSpan.Zero);

        _stationsRepository = stationsRepository;
        _timeProvider = timeProvider;
        _maxHeartbeatInterval = maxHeartbeatInterval;
        _logger = logger;
    }
    #endregion

    #region Service processing
    /// <summary>
    /// Determines which stations can be concidered as offline and marks them as such.
    /// </summary>
    /// <inheritdoc cref="IBackgroundServiceProcessor">
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset minHeartbeatTimestamp = _timeProvider.GetUtcNow() - _maxHeartbeatInterval;

        _logger.LogInformation(
            "Searching for stations to mark as offline: MinHeartbeatTimestamp=[{MinHeartbeatTimestamp}]",
            minHeartbeatTimestamp);
        
        long[] markedStationsIdentifiers = await _stationsRepository.MarkOfflineStations(minHeartbeatTimestamp);

        _logger.LogInformation(
            "Batch of stations marked as offline: Count=[{Count}], MinHeartbeatTimestamp=[{MinHeartbeatTimestamp}]", 
            markedStationsIdentifiers.Length,
            minHeartbeatTimestamp);

        markedStationsIdentifiers.ToList()
            .ForEach(stationId => 
                _logger.LogDebug(
                    "Station and all of its features marked as offline: StationId=[{Id}], MinHeartbeatTimestamp=[{MinHeartbeatTimestamp}]",
                    stationId,
                    minHeartbeatTimestamp));
    }
    #endregion
}
