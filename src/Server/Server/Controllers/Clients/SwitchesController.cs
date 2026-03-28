using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers;
using SmartHome.Server.Managers.Factories;
using System.Net;

namespace SmartHome.Server.Controllers.Clients;

/// <summary>
/// Controller dedicated to managing electrical switches.
/// </summary>
[ApiController]
[Route("api/clients/v1/switches")]
public class SwitchesController : BaseController
{
    #region Properties
    private readonly ISwitchesRepository _switchesRepository;
    private readonly ISwitchManagerFactory _switchManagerFactory;
    private readonly ILogger<SwitchesController> _logger;
    #endregion

    // TODO: Add unit tests.
    // TODO: Concider edge cases.
    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <param name="switchesRepository">
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <param name="switchManagerFactory">
    /// Factory which shall be used to create managers for performing operations on the switches.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchesController(
        IHttpContextAccessor httpContextAccessor,
        ISwitchesRepository switchesRepository,
        ISwitchManagerFactory switchManagerFactory,
        ILogger<SwitchesController> logger) : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(switchManagerFactory, nameof(switchManagerFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _switchesRepository = switchesRepository;
        _switchManagerFactory = switchManagerFactory;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// Retrieves all switches available in the repository.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of switches.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetSwitches()
    {
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning(
                "Request processing failed: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request to return collection of switches: ClientIpAddress=[{ClientIpAddress}]",
            clientIpAddress);

        SwitchEntity[] allSwitches = await _switchesRepository.GetMultipleSwitchesAsync();

        _logger.LogInformation(
            "Request processed successfully: ClientIpAddress=[{ClientIpAddress}, EntitiesReturned=[{EntitiesReturned}]",
            clientIpAddress,
            allSwitches.Count());

        return Ok(allSwitches);
    }

    /// <summary>
    /// Retrieves specified switch.
    /// </summary>
    /// <param name="switchId">
    /// The unique global identifier of the switch.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of switches.
    /// </returns>
    [HttpGet("{switchId}")]
    public async Task<IActionResult> GetSwitch(long switchId)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning(
                "Request processing failed: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request to return a switch: ClientIpAddress=[{ClientIpAddress}], SwitchId=[{SwitchId}]",
            clientIpAddress,
            switchId);

        SwitchEntity? switchEntity = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: switchId);

        _logger.Log(
            switchEntity is null ? LogLevel.Warning : LogLevel.Information,
            "Request processed successfully: ClientIpAddress=[{ClientIpAddress}], SwitchId=[{SwitchId}, EntitiesReturned=[{EntitiesReturned}]",
            clientIpAddress,
            switchId,
            switchEntity is null ? 0 : 1);

        return switchEntity is null ? NotFound() : Ok(switchEntity);
    }

    /// TODO: Add logging.
    /// <summary>
    /// Updates the data related to particular electrical switch according to details provided in request body.
    /// </summary>
    /// <param name="switchId">
    /// The unique global identifier of the switch.
    /// </param>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the switch which shall be updated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPatch("{switchId}")]
    public async Task<IActionResult> UpdateSwitch(
        long switchId,
        [FromBody] SwitchUpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        SwitchEntity? switchEntity = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: switchId);
    
        if (switchEntity is null)
        {
            return NotFound();
        }
    
        ISwitchManager switchManager = _switchManagerFactory.CreateFor(switchEntity);
    
        bool wasOperationSuccessful = await switchManager.TryChangeState(request.ExpectedSwitchState, cancellationToken);
    
        if (wasOperationSuccessful)
        {
            return NoContent();
        }
    
        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
