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
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning(
                "Station registration request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing station registration request: StationMacAddress=[{MacAddress}], StationIpAddress=[{StationIpAddress}]",
            request.StationMacAddress,
            stationIpAddress);

        _logger.LogDebug("Searching for station entity: StationMacAddress=[{StationMacAddress}]", request.StationMacAddress);

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.StationMacAddress);

        if (knownStationEntity is null)
        {
            _logger.LogDebug("Station entity not found: StationMacAddress=[{MacAddress}]", request.StationMacAddress);

            _logger.LogInformation(
                "Registering station as a new device within the system: StationMacAddress=[{MacAddress}], StationIpAddress=[{StationIpAddress}]",
                request.StationMacAddress,
                stationIpAddress);

            DateTimeOffset newStationHeartbeatTimestamp = _timeProvider.GetUtcNow();

            _logger.LogDebug("Creating new station entity: StationMacAddress=[{StationMacAddress}], StationIpAddress=[{IpAddress}], " +
                "StationApiPort=[{StationApiPort}], StationApiVersion=[{StationApiVersion}], LastHeartbeat=[{LastHeartbeat}]",
                request.StationMacAddress,
                stationIpAddress,
                request.StationApiPort,
                request.StationApiVersion,
                newStationHeartbeatTimestamp);

            StationEntity newStationEntity = await _stationsRepository.CreateStationAsync(
                request.StationMacAddress,
                stationIpAddress,
                request.StationApiPort,
                request.StationApiVersion,
                newStationHeartbeatTimestamp);

            _logger.LogDebug("Repository updated successfully: StationId=[{StationId}]", newStationEntity.Id);

            _logger.LogInformation(
                "Station registration successful: StationId=[{StationId}], StationIpAddress=[{StationIpAddress}]",
                newStationEntity.Id,
                newStationEntity.IpAddress);

            return NoContent();
        }

        _logger.LogDebug("Station entity found: StationId=[{Id}]", knownStationEntity.Id);

        _logger.LogInformation(
            "Registering station as already known device: StationId=[{Id}], StationIpAddress=[{StationIpAddress}]",
            knownStationEntity.Id,
            stationIpAddress);

        DateTimeOffset knownStationHeartbeatTimestamp = _timeProvider.GetUtcNow();

        _logger.LogDebug(
            "Updating station details: StationId=[{StationId}], StationIpAddress=[{IpAddress}], StationApiPort=[{StationApiPort}], " +
            "StationApiVersion=[{StationApiVersion}], LastHeartbeat=[{LastHeartbeat}]",
            knownStationEntity.Id,
            stationIpAddress,
            request.StationApiPort,
            request.StationApiVersion,
            knownStationHeartbeatTimestamp);

        StationEntity? updatedStationEntity = await _stationsRepository.UpdateStationAsync(
           knownStationEntity.Id,
           updateIpAddress: true,
           ipAddress: stationIpAddress,
           updateApiPort: true,
           apiPort: request.StationApiPort,
           updateApiVersion: true,
           apiVersion: request.StationApiVersion,
           updateLastHeartbeat: true,
           lastHeartbeat: knownStationHeartbeatTimestamp);

        if (updatedStationEntity is null)
        {
            _logger.LogError(
                "Failed to process station registration request: Message=[{Message}], StationId=[{StationId}], StationIpAddress=[{IpAddress}]",
                "Failed to update repository.",
                knownStationEntity.Id,
                stationIpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogDebug(
            "Repository updated successfully: StationId=[{StationId}]", updatedStationEntity.Id);

        _logger.LogInformation(
            "Station registration successful: StationId=[{StationId}], StationIpAddress=[{IpAddress}]",
            updatedStationEntity.Id,
            updatedStationEntity.IpAddress);

        return NoContent();
    }

    /// <summary>
    /// Updates timestamp of the last heartbeat received from the station calling the endpoint.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut("heartbeat")]
    public async Task<IActionResult> ProcessHeartbeatSignal()
    {
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning(
                "Processing heartbeat signal failed: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing heartbeat signal: StationIpAddress=[{StationIpAddress}]",
            stationIpAddress);

        _logger.LogDebug("Searching for station entity: StationIpAddress=[{StationIpAddress}]", stationIpAddress);

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (knownStationEntity is null)
        {
            _logger.LogWarning(
                "Processing heartbeat signal failed: Message=[{Message}], StationIpAddress=[{StationIpAddress}]",
                "Station entity not found.",
                stationIpAddress);

            return NotFound();
        }

        _logger.LogDebug("Station entity found: StationId=[{Id}]", knownStationEntity.Id);

        DateTimeOffset newHeartbeatTimestamp = _timeProvider.GetUtcNow();

        _logger.LogDebug(
            "Updating station details: StationId=[{Id}], LastHeartbeat=[{LastHeartbeat}]",
            knownStationEntity.Id, newHeartbeatTimestamp);

        StationEntity? updatedStationEntity = await _stationsRepository.UpdateStationAsync(
            knownStationEntity.Id,
            updateLastHeartbeat: true,
            lastHeartbeat: newHeartbeatTimestamp);

        if (updatedStationEntity is null)
        {
            _logger.LogError(
                "Failed to process heartbeat signal: Message=[{Message}], StationId=[{StationId}], StationIpAddress=[{IpAddress}]",
                 "Failed to update repository.",
                knownStationEntity.Id,
                stationIpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogDebug("Repository updated successfully: StationId=[{StationId}]", updatedStationEntity.Id);

        _logger.LogInformation(
            "Heartbeat signal processed successfully: StationId=[{StationId}], StationIpAddress=[{IpAddress}]",
            updatedStationEntity.Id,
            updatedStationEntity.IpAddress);

        return NoContent();
    }
}
