using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Controllers.Firmware;

/// <summary>
/// Controller providing endpoints for firmware to manage stations.
/// </summary>
[Route("api/firmware/v1/stations")]
public class StationsController : BaseController
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    private readonly TimeProvider _timeProvider;
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
    /// <param name="timeProvider">
    /// Time reference shall be used by the instance to coordinate time-based operations.
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
        TimeProvider timestampProvider,
        ILogger<StationsController> logger)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _stationsRepository = stationsRepository;
        _timeProvider = timestampProvider;
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
    [HttpPut]
    public async Task<IActionResult> RegisterStation([FromBody] StationRegistrationStationRequest request)
    {
        _logger.LogInformation("Processing station registration request: MacAddress=[{MacAddress}]", request.StationMacAddress);

        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning("Failed to process station registration request:");
            _logger.LogDebug("Failed to determine station IP address:");
            
            return BadRequest();
        }

        _logger.LogDebug("Station IP address determined: IpAddress=[{IpAddress}]", stationIpAddress);
        _logger.LogDebug("Searching for station entity: MacAddress=[{MacAddress}]", request.StationMacAddress);

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.StationMacAddress);

        if (knownStationEntity is null)
        {
            _logger.LogDebug("Station entity not found:");
            _logger.LogDebug("Registering station as a new device within the system:");

            await _stationsRepository.CreateStationAsync(
                request.StationMacAddress,
                stationIpAddress,
                request.StationApiPort,
                request.StationApiVersion,
                _timeProvider.GetUtcNow());

            _logger.LogInformation("Station registration successful:");

            return NoContent();
        }

        _logger.LogDebug("Station entity found: Id=[{Id}]", knownStationEntity.Id);
        _logger.LogDebug("Registering station as already known device:");

        DateTimeOffset heartbeatTimestamp = _timeProvider.GetUtcNow();

        _logger.LogDebug(
            "Updating station details: IpAddress=[{IpAddress}], LastHeartbeat=[{LastHeartbeat}]",
            stationIpAddress,
            heartbeatTimestamp);

        await _stationsRepository.UpdateStationAsync(
           knownStationEntity.Id,
           updateIpAddress: true,
           ipAddress: stationIpAddress,
           updateApiPort: true,
           apiPort: request.StationApiPort,
           updateApiVersion: true,
           apiVersion: request.StationApiVersion,
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

        DateTimeOffset heartbeatTimestamp = _timeProvider.GetUtcNow();

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
