using Microsoft.AspNetCore.Mvc;
using Server.Data.Repositories;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using System.Net;

namespace Server.Controllers;

/// <summary>
/// Controller dedicated to managing stations.
/// </summary>
[ApiController]
[Route("api/v1/stations")]
public class StationsController : ControllerBase
{
    #region Properties
    private readonly IStationsRepository _stationsRepository;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
    /// <param name="stationsRepository">
    /// Station repository which shall be managed by this controller.
    /// </param>
    public StationsController(IStationsRepository stationsRepository)
    {
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));

        _stationsRepository = stationsRepository;
    }
    #endregion

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
        ArgumentNullException.ThrowIfNull(stationDto, nameof(stationDto));

        IPAddress? stationIpAddress = HttpContext.Connection.RemoteIpAddress;
        
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
        return CreatedAtAction(nameof(RegisterStation), new { id = newStationEntity.Id }, newStationEntity.ToDto());
    }
}
