using Microsoft.AspNetCore.Mvc;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers;
using SmartHome.Server.Managers.Factories;

namespace SmartHome.Server.Controllers.Clients;

/// <summary>
/// Controller dedicated to managing electrical switches.
/// </summary>
[ApiController]
[Route("api/clients/v1/switches")]
public class SwitchesController : ControllerBase
{
    #region Properties
    private readonly ISwitchesRepository _switchesRepository;
    private readonly ISwitchManagerFactory _switchManagerFactory;
    private readonly ILogger<SwitchesController> _logger;
    #endregion

    // TODO: Add logging.
    // TODO: Add unit tests.
    // TODO: Concider edge cases.
    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
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
        ISwitchesRepository switchesRepository,
        ISwitchManagerFactory switchManagerFactory,
        ILogger<SwitchesController> logger)
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
        SwitchEntity[] allSwitches = await _switchesRepository.GetMultipleSwitchesAsync();
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
        SwitchEntity? switchEntity = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: switchId);

        if (switchEntity is null)
        {
            return NotFound();
        }

        return Ok(switchEntity);
    }

    /// <summary>
    /// Updates the data related to particular electrical switch according to details provided in request body.
    /// </summary>
    /// <param name="switchId">
    /// The unique global identifier of the switch.
    /// </param>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the switch which shall be updated.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPatch("{switchId}")]
    public async Task<IActionResult> UpdateSwitch(long switchId, [FromBody] SwitchUpdateClientRequest request)
    {
        SwitchEntity? switchEntity = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: switchId);
    
        if (switchEntity is null)
        {
            return NotFound();
        }
    
        ISwitchManager switchManager = _switchManagerFactory.CreateFor(switchEntity);
    
        bool wasOperationSuccessful = await switchManager.TryChangeState(request.ExpectedSwitchState);
    
        if (wasOperationSuccessful)
        {
            return NoContent();
        }
    
        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
