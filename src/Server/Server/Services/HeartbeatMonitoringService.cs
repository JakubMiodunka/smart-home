using SmartHome.Server.Data;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Services;

/// TODO: Add unit tests to this calss.
/// <summary>
/// Monitoring service that marks stations as offline if they fail 
/// to send a heartbeat signal within the configured time interval.
/// </summary>
public sealed class HeartbeatMonitoringService : BackgroundService
{
    #region Constraints
    // TODO: Move these values to some configuration file.
    private const int MaxHeartbeatInterval = 30_000;    // Given in milliseconds.
    private const int HeartbeatCheckInterval = 35_000;    // Given in milliseconds.
    #endregion

    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly ITimestampProvider _timestampProvider;
    private readonly TimeSpan _maxHeartbeatInterval;
    private readonly TimeSpan _heartbeatCheckInterval;
    private readonly ILogger<HeartbeatMonitoringService> _logger;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="HeartbeatMonitoringService"/>.
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
    public HeartbeatMonitoringService(
        IStationsRepository stationsRepository,
        ITimestampProvider timestampProvider,
        ILogger<HeartbeatMonitoringService> logger)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(HeartbeatCheckInterval, MaxHeartbeatInterval);

        _stationsRepository = stationsRepository;
        _timestampProvider = timestampProvider;
        _logger = logger;

        _maxHeartbeatInterval = TimeSpan.FromMilliseconds(MaxHeartbeatInterval);
        _heartbeatCheckInterval = TimeSpan.FromMilliseconds(HeartbeatCheckInterval);
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Runs the heartbeat monitoring loop as a long-running background task.
    /// </summary>
    /// <remarks>
    /// <param name="cancellationToken">
    /// Triggered when the application host is performing a graceful shutdown.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the background monitoring operation.
    /// </returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service started:");

        using var heartbeatIntervalTimer = new PeriodicTimer(_heartbeatCheckInterval);

        while (await heartbeatIntervalTimer.WaitForNextTickAsync(cancellationToken))
        {
            try
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
                    continue;
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
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception thrown in service execution loop.");
            }
        }
    }
    #endregion
}
