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
    /// Station repository which shall be managed by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public StationsController(IHttpContextAccessor httpContextAccessor, IStationsRepository stationsRepository)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));

        _httpContextAccessor = httpContextAccessor;
        _stationsRepository = stationsRepository;
    }
    #endregion

    // TODO: Refine this method.
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
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    [HttpPost]
    public async Task<IActionResult> RegisterStation([FromBody] StationDto stationDto)
    {
        if (stationDto is null)
        {
            return BadRequest("Station details must be provided in request body.");
        }

        IPAddress? stationIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        
        if (stationIpAddress is null)
        {
            return BadRequest("Unable to determine station IP address.");
        }

        StationEntity? stationEntity = await _stationsRepository.GetSingleStationAsync(filterByMacAddress: true, macAddress: stationDto.MacAddress);

        if (stationEntity is not null)
        {
            if (stationEntity.IpAddress == stationIpAddress)
            {
                return Ok(stationEntity.ToDto());
            }

            StationEntity? updatedStationEntity = await _stationsRepository.UpdateStationAsync(stationDto.MacAddress, updateIpAddress: true, ipAddress: stationIpAddress);
            return Ok(updatedStationEntity?.ToDto());
        }

        StationEntity newStationEntity = await _stationsRepository.CreateStationAsync(stationDto.MacAddress, stationIpAddress);
        return CreatedAtAction(nameof(GetStation), new { id = newStationEntity.Id }, newStationEntity.ToDto());
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
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetStation(long id)
    {
        StationEntity? stationEntity = await _stationsRepository.GetSingleStationAsync(filterById: true, id: id);
        return stationEntity is null ? NotFound() : Ok(stationEntity.ToDto());
    }
}
