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
    private readonly IStationsRepository _stationsRepository;
    private readonly ISwitchManagerFactory _switchManagerFactory;
    private readonly ILogger<SwitchesController> _logger;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <param name="switchesRepository">
    /// Switches repository which shall be used by this controller.
    /// </param>
    /// <param name="stationsRepository">
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
        IStationsRepository stationsRepository,
        ISwitchManagerFactory switchManagerFactory,
        ILogger<SwitchesController> logger) : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(switchManagerFactory, nameof(switchManagerFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _switchesRepository = switchesRepository;
        _stationsRepository = stationsRepository;
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
                "Request for getting collection of switches rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request for getting collection of switches: ClientIpAddress=[{ClientIpAddress}]",
            clientIpAddress);

        _logger.LogDebug("Searching for switch entities:");

        SwitchEntity[] allSwitches = await _switchesRepository.GetMultipleSwitchesAsync();

        _logger.LogDebug("Switch entities found: EntitiesReturned=[{EntitiesReturned}]", allSwitches.Count());

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
                "Request for getting a switch rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request for getting a switch: ClientIpAddress=[{ClientIpAddress}], SwitchId=[{SwitchId}]",
            clientIpAddress,
            switchId);

        _logger.LogDebug("Searching for switch entity: SwitchId=[{SwitchId}]", switchId);

        SwitchEntity? switchEntity = await _switchesRepository.GetSingleSwitchAsync(filterById: true, id: switchId);

        _logger.Log(
            switchEntity is null ? LogLevel.Warning : LogLevel.Information,
            "Request processed successfully: Message=[{Message}], ClientIpAddress=[{ClientIpAddress}], SwitchId=[{SwitchId}]",
            switchEntity is null ? "Switch not found." : "Switch found.",
            clientIpAddress,
            switchId);

        return switchEntity is null ? NotFound() : Ok(switchEntity);
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
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning("Switch update request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing switch update request: SwitchId=[{SwitchId}], " +
            "ExpectedSwitchState=[{ExpectedSwitchState}], ClientIpAddress=[{ClientIpAddress}]",
            switchId,
            request.ExpectedSwitchState,
            clientIpAddress);

        if (await _switchesRepository.GetSingleSwitchAsync(
                filterById: true,
                id: switchId) is not SwitchEntity switchEntity)
        {
            _logger.LogWarning(
                "Failed to process switch update request: Message=[{Message}], " +
                "SwitchId=[{SwitchId}], ClientIpAddress=[{ClientIpAddress}]",
                "Switch not found.",
                switchId,
                clientIpAddress);

            return NotFound();
        }

        if (await _stationsRepository.GetSingleStationAsync(
            filterById: true,
            id: switchEntity.StationId) is not StationEntity parentStation)
        {
            _logger.LogError(
                "Failed to process switch update request: Message=[{Message}], SwitchId=[{SwitchId}], " +
                "StationId=[{StationId}], ClientIpAddress=[{ClientIpAddress}]",
                "Parent station not found.",
                switchEntity.Id,
                switchEntity.StationId,
                clientIpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogDebug("Attempting to change switch state: SwitchId=[{SwitchId}], StationId=[{StationId}], " +
            "ExpectedSwitchState=[{ExpectedSwitchState}], ActualSwitchState=[{ActualSwitchState}]",
            switchEntity.Id,
            parentStation.Id,
            request.ExpectedSwitchState,
            switchEntity.ActualState);

        ISwitchManager switchManager = _switchManagerFactory.CreateFor(switchEntity, parentStation);

        if (await switchManager.TryChangeState(request.ExpectedSwitchState, cancellationToken))
        {
            _logger.LogDebug("Attempt to change switch state successful: SwitchId=[{SwitchId}]," +
                "StationId=[{StationId}], ActualSwitchState=[{ActualSwitchState}]",
                switchEntity.Id,
                switchEntity.StationId,
                request.ExpectedSwitchState);

            if (await _switchesRepository.UpdateSwitchAsync(
                switchEntity.Id,
                updateExpectedState: true,
                expectedState: request.ExpectedSwitchState,
                updateActualState: true,
                actualState: request.ExpectedSwitchState) is null)
            {
                _logger.LogError(
                    "Failed to process switch update request: Message=[{Message}], SwitchId=[{SwitchId}], " +
                    "StationId=[{StationId}], ClientIpAddress=[{ClientIpAddress}]",
                    "Switches repository update failed.",
                    switchEntity.Id,
                    parentStation.Id,
                    clientIpAddress);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation(
                "Switch update request processed successfully: Message=[{Message}], SwitchId=[{SwitchId}]," +
                "StationId=[{StationId}], ActualSwitchState=[{ActualSwitchState}], ClientIpAddress=[{ClientIpAddress}]",
                "Actual switch state changed.",
                switchEntity.Id,
                parentStation.Id,
                request.ExpectedSwitchState,
                clientIpAddress);

            return NoContent();
        }

        _logger.LogDebug(
            "Unable to change actual switch state: SwitchId=[{SwitchId}], StationId=[{StationId}]",
            switchEntity.Id,
            parentStation.Id);

        if (await _switchesRepository.UpdateSwitchAsync(
                switchEntity.Id,
                updateExpectedState: true,
                expectedState: request.ExpectedSwitchState) is null)
        {
            _logger.LogError(
                "Failed to process switch update request: Message=[{Message}], SwitchId=[{SwitchId}], " +
                "StationId=[{StationId}], ClientIpAddress=[{ClientIpAddress}]",
                "Switches repository update failed.",
                switchEntity.Id,
                parentStation.Id,
                clientIpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogInformation(
            "Switch update request processed successfully: Message=[{Message}], SwitchId=[{SwitchId}], " +
            "StationId=[{StationId}], ExpectedSwitchState=[{ExpectedSwitchState}], ClientIpAddress=[{ClientIpAddress}]",
            "Expected switch state changed.",
            switchEntity.Id,
            parentStation.Id,
            request.ExpectedSwitchState,
            clientIpAddress);

        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
