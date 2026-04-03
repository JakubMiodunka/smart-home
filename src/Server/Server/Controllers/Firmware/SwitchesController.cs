using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Models.Requests;
using SmartHome.Server.Data.Models.Responses;
using SmartHome.Server.Data.Repositories;
using System.Net;

namespace SmartHome.Server.Controllers.Firmware;

/// <summary>
/// Controller dedicated to managing electrical switches.
/// </summary>
[Route("api/firmware/v1/switches")]
public class SwitchesController : BaseController
{
    #region Constraints
    private const bool DefaultExpectedSwitchState = false;
    private readonly bool? DefaultActualSwitchState = null;
    #endregion

    #region Properties
    private readonly ISwitchesRepository _switchesRepository;
    private readonly IStationsRepository _stationsRepository;
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
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this controller.
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
        ILogger<SwitchesController> logger)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _switchesRepository = switchesRepository;
        _stationsRepository = stationsRepository;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// Registers an electrical switch within the system using details provided in request body.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about electrical switch which shall be registered within the system.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> RegisterSwitch([FromBody] SwitchRegistrationStationRequest request)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning(
                "Switch registration request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing switch registration request: StationIpAddress=[{StationIpAddress}], SwitchLocalId=[{SwitchLocalId}]",
            stationIpAddress,
            request.SwitchLocalId);

        _logger.LogDebug(
            "Searching for parent station entity: StationIpAddress=[{StationIpAddress}]",
            stationIpAddress);

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (parentStationEntity is null)
        {
            _logger.LogWarning(
                "Failed to process switch registration request: Message=[{Message}], StationIpAddress=[{StationIpAddress}]",
                "Parent station entity not found.",
                stationIpAddress);

            return NotFound();
        }

        _logger.LogDebug("Parent station entity found: StationId=[{Id}]", parentStationEntity.Id);

        _logger.LogDebug(
            "Searching for switch entity: StationId=[{StationId}], LocalId=[{LocalId}]",
            parentStationEntity.Id,
            request.SwitchLocalId);

        SwitchEntity? switchEntity = 
            await _switchesRepository.GetSingleSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SwitchLocalId);

        if (switchEntity is null)
        {
            _logger.LogDebug(
                "Switch entity not found: StationId=[{StationId}], LocalId=[{LocalId}]",
                parentStationEntity.Id,
                request.SwitchLocalId);

            _logger.LogInformation(
                "Registering switch as a new device within the system: StationId=[{StationId}], StationIpAddress=[{StationIpAddress}], LocalId=[{LocalId}]",
                parentStationEntity.Id,
                parentStationEntity.IpAddress,
                request.SwitchLocalId);

            _logger.LogDebug(
                "Creating new switch entity: StationId=[{StationId}], LocalId=[{LocalId}], ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                parentStationEntity.Id,
                request.SwitchLocalId,
                DefaultExpectedSwitchState,
                DefaultActualSwitchState);

            switchEntity = await _switchesRepository.CreateSwitchAsync(
                parentStationEntity.Id,
                request.SwitchLocalId,
                DefaultExpectedSwitchState,
                actualState: DefaultActualSwitchState);

            _logger.LogDebug("Repository updated successfully: SwitchId=[{Id}]", switchEntity.Id);
        }
        else
        {
            _logger.LogDebug("Switch entity found: SwitchId=[{Id}]", switchEntity.Id);

            _logger.LogInformation(
                "Registering switch as already known device: SwitchId=[{Id}], StationIpAddress=[{StationIpAddress}]",
                switchEntity.Id,
                parentStationEntity.IpAddress);

            // Nothing to be done.
        }

        _logger.LogInformation(
            "Switch registration successful: SwitchId=[{Id}], StationIpAddress=[{StationIpAddress}]",
            switchEntity.Id,
            parentStationEntity.IpAddress);

        var response = new SwitchRegistrationServerResponse(switchEntity.Id, switchEntity.ExpectedState);
        return Ok(response);
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
    public async Task<IActionResult> UpdateSwitch(long switchId, [FromBody] SwitchUpdateStationRequest request)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning(
                "Switch update request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing switch update request: SwitchId=[{SwitchId}], StationIpAddress=[{StationIpAddress}]",
            switchId,
            stationIpAddress);

        _logger.LogDebug("Searching for parent station entity: StationIpAddress=[{StationIpAddress}]", stationIpAddress);

        StationEntity? stationEntity = 
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (stationEntity is null)
        {
            _logger.LogWarning("Failed to process switch update request: Message=[{Message}], SwitchId=[{SwitchId}], StationIpAddress=[{StationIpAddress}]",
                "Parent station entity not found.",
                switchId,
                stationIpAddress);

            return NotFound();
        }

        _logger.LogDebug("Parent station entity found: StationId=[{StationId}]", stationEntity.Id);

        _logger.LogDebug(
            "Searching for switch entity: StationId=[{StationId}], SwitchId=[{SwitchId}]",
            stationEntity.Id,
            switchId);

        SwitchEntity? switchEntity =
            await _switchesRepository.GetSingleSwitchAsync(
                filterById: true,
                id: switchId,
                filterByStationId: true,
                stationId: stationEntity.Id);

        if (switchEntity is null)
        {
            _logger.LogWarning("Failed to process switch update request: Message=[{Message}], " +
                "SwitchId=[{SwitchId}], StationId=[{StationId}], StationIpAddress=[{StationIpAddress}]",
                "Switch entity not found.",
                switchId,
                stationEntity.Id,
                stationIpAddress);

            return NotFound();
        }

        _logger.LogDebug("Switch entity found: SwitchId=[{SwitchId}]", switchEntity.Id);

        _logger.LogDebug(
            "Updating switch details: SwitchId=[{SwitchId}], ActualSwitchState=[{ActualSwitchState}]",
            switchEntity.Id,
            request.ActualSwitchState);

        SwitchEntity? updatedSwitchEntity = await _switchesRepository.UpdateSwitchAsync(
            switchEntity.Id,
            updateActualState: true,
            actualState: request.ActualSwitchState);

        if (updatedSwitchEntity is null)
        {
            _logger.LogError(
                "Failed to process switch update request: Message=[{Message}], SwitchId=[{SwitchId}], " +
                "StationId=[{StationId}], StationIpAddress=[{StationIpAddress}]",
                "Failed to update repository.",
                switchEntity.Id,
                stationEntity.Id,
                stationEntity.IpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogDebug("Repository updated successfully: SwitchId=[{SwitchId}]", switchEntity.Id);

        _logger.LogInformation("Switch update request processed successfully: SwitchId=[{SwitchId}], " +
            "StationId=[{StationId}], StationIpAddress=[{StationIpAddress}]",
            switchEntity.Id,
            stationEntity.Id,
            stationEntity.IpAddress);

        return NoContent();
    }
}
