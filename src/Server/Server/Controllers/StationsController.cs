using Microsoft.AspNetCore.Mvc;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Controllers;

/// <summary>
/// Controller dedicated to managing stations.
/// </summary>
[ApiController]
[Route("api/v1/stations")]
public class StationsController : ControllerBase
{
    #region Properties
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStationsRepository _stationsRepository;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public StationsController(IHttpContextAccessor httpContextAccessor, IStationsRepository stationsRepository)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
        _stationsRepository = stationsRepository;
    }
    #endregion

    /// <summary>
    /// Attempts to retrieve the remote IP address of the client from the current HTTP context.
    /// </summary>
    /// <param name="ipAddress">
    /// Contains the remote IP address of the client if attempt was successful,
    /// <see langword="null"/> otherwise.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the IP address was successfully retrieved,
    /// <see langword="false"/> otherwise.
    /// </returns>
    private bool TryGetRemoteIpAddress(out IPAddress? ipAddress)
    {
        ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        return ipAddress is not null;
    }

    /// <summary>
    /// Registers a station within the system using details provided in request body.
    /// If the station is known to the server, details related to it will be updated on the server site accordingly.
    /// </summary>
    /// <param name="stationDto">
    /// Data transfer object (DTO) representing the general details about station, which calls the endpoint.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> RegisterStation([FromBody] StationDto stationDto)
    {       
        if (!TryGetRemoteIpAddress(out IPAddress? remoteIpAddress))
        {
            return BadRequest("Unable to determine remote IP address.");
        }

        StationEntity? knownStationEntity = await _stationsRepository.GetSingleStationAsync(
            filterByMacAddress: true,
            macAddress: stationDto.MacAddress);

        if (knownStationEntity is null)
        {
            StationEntity createdStationEntity = await _stationsRepository.CreateStationAsync(
                stationDto.MacAddress,
                remoteIpAddress);

            return CreatedAtAction(nameof(GetStation), new { id = createdStationEntity.Id }, createdStationEntity.ToDto());
        }

        if (knownStationEntity.IpAddress == remoteIpAddress)
        {
            return Ok(knownStationEntity.ToDto());
        }

        StationEntity? updatedStationEntity = await _stationsRepository.UpdateStationAsync(
            knownStationEntity.Id,
            updateIpAddress: true,
            ipAddress: remoteIpAddress);

        return Ok(updatedStationEntity?.ToDto());
    }

    /// <summary>
    /// Retrieves details about a specific station.
    /// </summary>
    /// <param name="id">
    /// Identifier of the station to be retrieved.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpGet("{id:long:min(1)}")]
    public async Task<IActionResult> GetStation(long id)
    {
        StationEntity? stationEntity = await _stationsRepository.GetSingleStationAsync(filterById: true, id: id);
        return stationEntity is null ? NotFound() : Ok(stationEntity.ToDto());
    }
}
