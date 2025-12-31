using Microsoft.AspNetCore.Mvc;
using Server.Data.Models.Dtos;
using Server.Data.Models.Entities;
using Server.Data.Repositories;

namespace Server.Controllers;

[ApiController]
[Route("api/v1/stations")]
public class StationsController : ControllerBase
{
    private readonly IStationRepository _stationRepository;

    public StationsController(IStationRepository stationRepository)
    {
        _stationRepository = stationRepository;
    }

    [HttpPost]
    public async Task<IActionResult> ReqisterStation([FromBody] StationDto stationDto)
    {
        if (await _stationRepository.IsStationExistAsync(stationDto.MacAddress))
        {
            return Ok();
        }

        // TODO: Continue here.
    }
}
