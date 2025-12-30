using Microsoft.AspNetCore.Mvc;
using Server.Data.Models.Requests;

namespace Server.Controllers;

[ApiController]
[Route("api/v1/stations")]
public class StationsController : ControllerBase
{
    [HttpPost]
    public IActionResult ReqisterStation([FromBody] RegisterStationRequest request)
    {
        // TODO: Continue here.
        return NotFound();
    }
}
