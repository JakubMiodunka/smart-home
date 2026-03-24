using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Services.Processors;

/// <summary>
/// A processor responsible for monitoring station heartbeats and marking inactive stations as offline.
/// </summary>
/// <remarks>
/// This class encapsulates the business logic for determining station timeouts. 
/// It is intended to be executed periodically by a <see cref="BackgroundProcessorService"> instance.
/// </remarks>
public sealed class HeartbeatMonitoringServiceProcessor : IBackgroundServiceProcessor
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly ISwitchesRepository _switchesRepository;
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
    /// <param name="switchesRepository">
    /// Switches repository which shall be used by created instance.
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
        ISwitchesRepository switchesRepository,
        TimeProvider timeProvider,
        TimeSpan maxHeartbeatInterval,
        ILogger<HeartbeatMonitoringServiceProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxHeartbeatInterval, TimeSpan.Zero);

        _stationsRepository = stationsRepository;
        _switchesRepository = switchesRepository;
        _timeProvider = timeProvider;
        _maxHeartbeatInterval = maxHeartbeatInterval;
        _logger = logger;
    }
    #endregion

    #region Service processing

    /// <summary>
    /// Searching for stations which shall be marked as offline in repository.
    /// </summary>
    /// <param name="referenceTimestamp">
    /// Timestamp against which station activity is compared to determine offline state.
    /// </param>
    /// <returns>
    /// Collection of statin entities which shall be marked as offline.
    /// </returns>
    private async Task<StationEntity[]> FindStationsToMarkAsOffline(DateTimeOffset referenceTimestamp)
    {
        _logger.LogDebug("Searching for stations to mark as offline: ReferenceTimestamp=[{ReferenceTimestamp}]", referenceTimestamp);

        StationEntity[] allStations = await _stationsRepository.GetMultipleStationsAsync();
        
        StationEntity[] stationsToMark = allStations
            .Where(station => station.IsOnline() is true or null)   // Stations with unclear status are also processed to ensure data integrity by marking them offline.
            .Where(station => referenceTimestamp - station.LastHeartbeat > _maxHeartbeatInterval)
            .ToArray();

        _logger.LogDebug("Stations found to mark as offline: Count=[{Count}]", stationsToMark.Count());

        return stationsToMark;
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

        StationEntity? updatedStation = await _stationsRepository.UpdateStationAsync(
            stationId,
            updateIpAddress: true,
            ipAddress: null,
            updateApiPort: true,
            apiPort: null,
            updateApiVersion: true,
            apiVersion: null);

        if (updatedStation is null)
        {
            _logger.LogError("Failed to update repository:");
            return;
        }

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
    private async Task MarkStationSwitchesAsOffline(long parentStationId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(parentStationId, 0, nameof(parentStationId));

        _logger.LogDebug("Searching for switches to mark as offline: ParentStationId=[{ParentStationId}]", parentStationId);

        SwitchEntity[] stationSwitches = await _switchesRepository.GetMultipleSwitchesAsync(
            filterByStationId: true,
            stationId: parentStationId);

        SwitchEntity[] switchesToMark = stationSwitches
            .Where(currentSwitch => currentSwitch.IsOnline())
            .ToArray();

        if (switchesToMark.Any())
        {
            _logger.LogDebug("Found switches to mark as offline: Count=[{Count}]", stationSwitches.Count());
        }
        else
        {
            _logger.LogDebug("Stations to mark as offline not found: Count=[{Count}]", stationSwitches.Count());
            return;
        }

        // List.ForEach is avoided here as it does not support asynchronous operations.
        foreach (SwitchEntity switchToMark in switchesToMark)
        {
            _logger.LogDebug("Marking switch as offline: Id=[{Id}]", switchToMark.Id);

            SwitchEntity? updatedSwitch = await _switchesRepository.UpdateSwitchAsync(
                switchToMark.Id,
                updateActualState: true,
                actualState: null);

            if (updatedSwitch is null)
            {
                _logger.LogError("Failed to update repository:");
                continue;
            }

            _logger.LogDebug("Repository updated successfully:");
        }
    }

    /// <inheritdoc cref="IBackgroundServiceProcessor">
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset referenceTimestamp = _timeProvider.GetUtcNow();
        StationEntity[] stationsToMark = await FindStationsToMarkAsOffline(referenceTimestamp);

        // List.ForEach is avoided here as it does not support asynchronous operations.
        foreach (StationEntity station in stationsToMark)
        {
            await MarkStationAsOffline(station.Id);
            await MarkStationSwitchesAsOffline(station.Id);
        }
    }
    #endregion
}
