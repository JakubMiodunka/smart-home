using Microsoft.AspNetCore.Mvc;
using Server.Data.Repositories;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using System.Net;

namespace Server.Controllers;

[ApiController]
[Route("api/v1/stations")]
public class StationsController : ControllerBase
{
    private readonly IStationsRepository _stationsRepository;

    public StationsController(IStationsRepository stationsRepository) =>
        _stationsRepository = stationsRepository;

    [HttpPost]
    public async Task<IActionResult> RegisterStation([FromBody] StationDto stationDto)
    {
        StationEntity? knownStation = await _stationsRepository.GetSingleStationAsync(macAddress: stationDto.MacAddress, ignoreMacAddress: false);
        bool isKnownStation = knownStation is not null;
        IPAddress? stationIpAddress = HttpContext.Connection.RemoteIpAddress;

        if (isKnownStation)
        {
            StationEntity? updatedStation = await _stationsRepository.UpdateStationAsync(stationDto.MacAddress, ipAddress: stationIpAddress, ignoreIpAddress: false);
            return Ok(updatedStation?.ToDto());
        }

        StationEntity newStationEntity = await _stationsRepository.CreateStationAsync(stationDto.MacAddress, stationIpAddress);
        return CreatedAtAction(nameof(RegisterStation), new { id = newStationEntity.Id }, newStationEntity.ToDto());
    }
}
