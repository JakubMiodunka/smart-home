using SmartHome.Server.Data;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Services.Processors;

/// TODO: Add unit tests to this calss.
/// <summary>
/// A processor responsible for monitoring station heartbeats and marking inactive stations as offline.
/// </summary>
/// <remarks>
/// This class encapsulates the business logic for determining station timeouts. 
/// It is intended to be executed periodically by a <see cref="BackgroundServiceProcessorWrapper"> instance.
/// </remarks>
public sealed class HeartbeatMonitoringServiceProcessor : IBackgroundServiceProcessor
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly ITimestampProvider _timestampProvider;
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
    /// <param name="timestampProvider">
    /// Timestamp source which shall be used by created instance.
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
        ITimestampProvider timestampProvider,
        TimeSpan maxHeartbeatInterval,
        ILogger<HeartbeatMonitoringServiceProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxHeartbeatInterval, TimeSpan.Zero);

        _stationsRepository = stationsRepository;
        _timestampProvider = timestampProvider;
        _maxHeartbeatInterval = maxHeartbeatInterval;
        _logger = logger;
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="IBackgroundServiceProcessor">
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        StationEntity[] allStations = await _stationsRepository.GetMultipleStationsAsync();
        DateTime currentTimestamp = _timestampProvider.GetUtcNow();

        long[] timedOutStationIds = allStations
            .Where(station => station.IsOnline())
            .Where(station => currentTimestamp - station.LastHeartbeat > _maxHeartbeatInterval)
            .Select(station => station.Id)
            .ToArray();

        if (!timedOutStationIds.Any())
        {
            return;
        }

        _logger.LogDebug("Found stations to mark as offline: Count=[{Count}]", timedOutStationIds.Count());

        // List.ForEach is avoided here as it does not support asynchronous operations.
        foreach (long id in timedOutStationIds)
        {
            await _stationsRepository.UpdateStationAsync(
                id,
                updateIpAddress: true,
                ipAddress: null);

            _logger.LogDebug("Station marked as offline: Id=[{Id}]", id);
        }
    }
    #endregion
}
