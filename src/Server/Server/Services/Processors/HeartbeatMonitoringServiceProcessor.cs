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
    private readonly ISwitchesRepository _switchesRepository;
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
    /// <param name="switchesRepository">
    /// Switches repository which shall be used by created instance.
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
        ISwitchesRepository switchesRepository,
        ITimestampProvider timestampProvider,
        TimeSpan maxHeartbeatInterval,
        ILogger<HeartbeatMonitoringServiceProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxHeartbeatInterval, TimeSpan.Zero);

        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        _timestampProvider = timestampProvider;
        _maxHeartbeatInterval = maxHeartbeatInterval;
        _logger = logger;
    }
    #endregion

    #region Service processing

    /// <summary>
    /// Searching for stations which shall be marked as offline in repository.
    /// </summary>
    /// <returns>
    /// Collection of statin entities which shall be marked as offline.
    /// </returns>
    private async Task<StationEntity[]> FindOfflineStations()
    {
        DateTime referenceTimestamp = _timestampProvider.GetUtcNow();
        _logger.LogDebug("Searching for stations to mark as offline: ReferenceTimestamp=[{ReferenceTimestamp}]", referenceTimestamp);

        StationEntity[] allStations = await _stationsRepository.GetMultipleStationsAsync();
        StationEntity[] offlineStations = allStations
            .Where(station => station.IsOnline())
            .Where(station => referenceTimestamp - station.LastHeartbeat > _maxHeartbeatInterval)
            .ToArray();

        if (offlineStations.Any())
        {
            _logger.LogDebug("Found stations to mark as offline: Count=[{Count}]", offlineStations.Count());
        }
        else
        {
            _logger.LogDebug("Stations to mark as offline not found: Count=[{Count}]", offlineStations.Count());
        }

        return offlineStations;
    }

    /// <summary>
    /// Marks specified station as offline.
    /// </summary>
    /// <param name="stationId">
    /// ID of station which shall be marked as offline.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value of at least one argument is outside its valid range.
    /// </exception>
    private async Task MarkStationAsOffline(long stationId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(stationId, 0, nameof(stationId));

        _logger.LogDebug("Marking station as offline: Id=[{Id}]", stationId);

        await _stationsRepository.UpdateStationAsync(
            stationId,
            updateIpAddress: true,
            ipAddress: null);

        _logger.LogDebug("Repository updated successfully:");
    }

    /// <summary>
    /// Marks all switches controlled by specified station as offline.
    /// </summary>
    /// <param name="parentStationId">
    /// The unique identifier of the station whose switches are to be updated.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value of at least one argument is outside its valid range.
    /// </exception>
    private async Task MarkSwitchesAsOffline(long parentStationId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(parentStationId, 0, nameof(parentStationId));

        _logger.LogDebug("Searching for switches to mark as offline: ParentStationId=[{ParentStationId}]", parentStationId);

        SwitchEntity[] offlineSwitches = await _switchesRepository.GetMultipleSwitchesAsync(
            filterByStationId: true,
            stationId: parentStationId,
            filterByActualState: true,
            actualState: null);

        if (offlineSwitches.Any())
        {
            _logger.LogDebug("Found switches to mark as offline: Count=[{Count}]", offlineSwitches.Count());
        }
        else
        {
            _logger.LogDebug("Stations to mark as offline not found: Count=[{Count}]", offlineSwitches.Count());
            return;
        }

        // List.ForEach is avoided here as it does not support asynchronous operations.
        foreach (SwitchEntity offlineSwitch in offlineSwitches)
        {
            _logger.LogDebug("Marking switch as offline: Id=[{Id}]", offlineSwitch.Id);

            await _switchesRepository.UpdateSwitchAsync(
                offlineSwitch.Id,
                updateActualState: true,
                actualState: null);

            _logger.LogDebug("Repository updated successfully:");
        }
    }

    /// <inheritdoc cref="IBackgroundServiceProcessor">
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        StationEntity[] offlineStations = await FindOfflineStations();

        // List.ForEach is avoided here as it does not support asynchronous operations.
        foreach (StationEntity station in offlineStations)
        {
            await MarkStationAsOffline(station.Id);
            await MarkSwitchesAsOffline(station.Id);
        }
    }
    #endregion
}
