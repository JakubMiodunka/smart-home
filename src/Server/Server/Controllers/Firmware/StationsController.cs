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
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public StationsController(
        IHttpContextAccessor httpContextAccessor,
        IStationsRepository stationsRepository,
        ITimestampProvider timestampProvider)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(timestampProvider, nameof(timestampProvider));

        _stationsRepository = stationsRepository;
        _timestampProvider = timestampProvider;
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
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            return BadRequest("Unable to determine station IP address.");
        }

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.MacAddress);

        if (knownStationEntity is null)
        {
            await _stationsRepository.CreateStationAsync(
                request.MacAddress,
                stationIpAddress,
                _timestampProvider.GetUtcNow());
        }
        else
        {
            await _stationsRepository.UpdateStationAsync(
               knownStationEntity.Id,
               updateIpAddress: true,
               ipAddress: stationIpAddress,
               updateLastHeartbeat: true,
               lastHeartbeat: _timestampProvider.GetUtcNow());
        }

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
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            return BadRequest("Unable to determine station IP address.");
        }

        StationEntity? knownStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (knownStationEntity is null)
        {
            return NotFound("Station with provided IP address is not registered.");
        }

        await _stationsRepository.UpdateStationAsync(
            knownStationEntity.Id,
            updateLastHeartbeat: true,
            lastHeartbeat: _timestampProvider.GetUtcNow());

        return NoContent();
    }
}
