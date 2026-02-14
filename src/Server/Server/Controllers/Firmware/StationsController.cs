using Microsoft.AspNetCore.Mvc;
using SmartHome.Server.Data;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Controllers.Firmware;

/// <summary>
/// Controller providing endpoints for firmware to manage stations.
/// </summary>
[Route("api/firmware/v1/stations")]
public class StationsController : FirmwareController
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly ITimestampProvider _timestampProvider;
    private readonly ILogger<StationsController> _logger;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new instance of <see cref="StationsController"/>".
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <param name="timestampProvider">
    /// Timestamp source which shall be used by this controller.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public StationsController(
        IHttpContextAccessor httpContextAccessor,
        IStationsRepository stationsRepository,
        ITimestampProvider timestampProvider,
        ILogger<StationsController> logger)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _stationsRepository = stationsRepository;
        _timestampProvider = timestampProvider;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// Registers a station within the system using details provided in request body.
    /// If the station is known to the server, details related to it will be updated on the server site accordingly.
    /// </summary>
    /// <param name="request">
    /// Registration request sent by station calling the endpoint.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut("registration")]
    public async Task<IActionResult> RegisterStation([FromBody] StationRegistrationRequest request)
    {
        _logger.LogInformation("Processing station registration request: MacAddress=[{MacAddress}]", request.MacAddress);

        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning("Failed to process station registration request:");
            _logger.LogDebug("Failed to determine station IP address:");
            
            return BadRequest();
        }

        _logger.LogDebug("Station IP address determined: IpAddress=[{IpAddress}]", stationIpAddress);
        _logger.LogDebug("Searching for station entity: MacAddress=[{MacAddress}]", request.MacAddress);

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.MacAddress);

        if (knownStationEntity is null)
        {
            _logger.LogDebug("Station entity not found:");
            _logger.LogDebug("Registering station as a new device within the system:");

            await _stationsRepository.CreateStationAsync(
                request.MacAddress,
                stationIpAddress,
                _timestampProvider.GetUtcNow());

            _logger.LogInformation("Station registration successful:");

            return NoContent();
        }

        _logger.LogDebug("Station entity found: Id=[{Id}]", knownStationEntity.Id);
        _logger.LogDebug("Registering station as already known device:");

        DateTime heartbeatTimestamp = _timestampProvider.GetUtcNow();

        _logger.LogDebug(
            "Updating station details: IpAddress=[{IpAddress}], LastHeartbeat=[{LastHeartbeat}]",
            stationIpAddress,
            heartbeatTimestamp);

        await _stationsRepository.UpdateStationAsync(
           knownStationEntity.Id,
           updateIpAddress: true,
           ipAddress: stationIpAddress,
           updateLastHeartbeat: true,
           lastHeartbeat: heartbeatTimestamp);

        _logger.LogDebug("Repository updated successfully:");
        _logger.LogInformation("Station registration successful:");

        return NoContent();
    }

    /// <summary>
    /// Updates timestamp of the last heartbeat received from the station calling the endpoint.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut("heartbeat")]
    public async Task<IActionResult> UpdateHeartbeatTimestamp()
    {
        _logger.LogInformation("Processing heartbeat signal:");

        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning("Failed to process heartbeat signal:");
            _logger.LogDebug("Failed to determine station IP address:");

            return BadRequest();
        }

        _logger.LogDebug("Station IP address determined: IpAddress=[{IpAddress}]", stationIpAddress);
        _logger.LogDebug("Searching for station entity: IpAddress=[{IpAddress}]", stationIpAddress);

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (knownStationEntity is null)
        {
            _logger.LogWarning("Failed to process heartbeat signal:");
            _logger.LogDebug("Station entity not found:");

            return NotFound();
        }

        _logger.LogDebug("Station entity found: Id=[{Id}]", knownStationEntity.Id);

        DateTime heartbeatTimestamp = _timestampProvider.GetUtcNow();

        _logger.LogDebug("Updating station details: Timestamp=[{Timestamp}]", heartbeatTimestamp);

        await _stationsRepository.UpdateStationAsync(
            knownStationEntity.Id,
            updateLastHeartbeat: true,
            lastHeartbeat: heartbeatTimestamp);

        _logger.LogDebug("Repository updated successfully:");
        _logger.LogInformation("Heartbeat signal processed successfully:");

        return NoContent();
    }
}
