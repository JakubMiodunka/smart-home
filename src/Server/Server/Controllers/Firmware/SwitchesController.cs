using Microsoft.AspNetCore.Mvc;
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
public class SwitchesController : FirmwareController
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
    [HttpPut("registration")]
    public async Task<IActionResult> RegisterSwitch([FromBody] SwitchRegistrationRequest request)
    {
        _logger.LogInformation(
            "Processing switch registration request: SwitchLocalId=[{SwitchLocalId}]",
            request.SwitchLocalId);

        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning("Failed to process switch registration request:");
            _logger.LogDebug("Failed to determine parent station IP address:");

            return BadRequest();
        }

        _logger.LogDebug("Parent station IP address determined: IpAddress=[{IpAddress}]", stationIpAddress);
        _logger.LogDebug("Searching for parent station entity: IpAddress=[{IpAddress}]", stationIpAddress);

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (parentStationEntity is null)
        {
            _logger.LogWarning("Failed to process switch registration request:");
            _logger.LogDebug("Parent station entity not found:");

            return NotFound();
        }

        _logger.LogDebug("Parent station entity found: Id=[{Id}]", parentStationEntity.Id);

        _logger.LogDebug(
            "Searching for switch entity: StationId=[{StationId}], LocalId=[{LocalId}]",
            parentStationEntity.Id,
            request.SwitchLocalId);

        SwitchEntity? knownSwitchEntity = 
            await _switchesRepository.GetSingleSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SwitchLocalId);

        SwitchRegistrationResponse response;

        if (knownSwitchEntity is null)
        {
            _logger.LogDebug("Switch entity not found:");
            _logger.LogDebug("Registering switch as a new device within the system:");

            _logger.LogDebug(
                "Creating new switch entity: StationId=[{StationId}], LocalId=[{LocalId}], ExpectedState=[{ExpectedState}], ActualState=[{ActualState}]",
                parentStationEntity.Id,
                request.SwitchLocalId,
                DefaultExpectedSwitchState,
                DefaultActualSwitchState);

            SwitchEntity newSwitchEntity = await _switchesRepository.CreateSwitchAsync(
                parentStationEntity.Id,
                request.SwitchLocalId,
                DefaultExpectedSwitchState,
                actualState: DefaultActualSwitchState);

            _logger.LogDebug("Switch entity created successfully: Id=[{Id}]", newSwitchEntity.Id);

            response = new SwitchRegistrationResponse(DefaultExpectedSwitchState);
        }
        else
        {
            _logger.LogDebug("Switch entity found: Id=[{Id}]", knownSwitchEntity.Id);
            _logger.LogDebug("Registering switch as already known device:");

            response = new SwitchRegistrationResponse(knownSwitchEntity.ExpectedState);
        }

        _logger.LogInformation("Switch registration successful:");

        return Ok(response);
    }

    /// <summary>
    /// Updates the data related to particular electrical switch according to details provided in request body.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the switch which shall be updated.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPatch("state")]
    public async Task<IActionResult> UpdateSwitch([FromBody] SwitchUpdateRequest request)
    {
        _logger.LogInformation(
            "Processing switch update request: SwitchLocalId=[{SwitchLocalId}]",
            request.SwitchLocalId);

        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning("Failed to process switch update request:");
            _logger.LogDebug("Failed to determine parent station IP address:");

            return BadRequest();
        }

        _logger.LogDebug("Parent station IP address determined: IpAddress=[{IpAddress}]", stationIpAddress);
        _logger.LogDebug("Searching for parent station entity: IpAddress=[{IpAddress}]", stationIpAddress);

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress);

        if (parentStationEntity is null)
        {
            _logger.LogWarning("Failed to process switch update request:");
            _logger.LogDebug("Parent station entity not found:");

            return NotFound();
        }

        _logger.LogDebug("Parent station entity found: Id=[{Id}]", parentStationEntity.Id);

        _logger.LogDebug(
            "Searching for switch entity: StationId=[{StationId}], LocalId=[{LocalId}]",
            parentStationEntity.Id,
            request.SwitchLocalId);

        SwitchEntity? knownSwitchEntity =
            await _switchesRepository.GetSingleSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SwitchLocalId);

        if (knownSwitchEntity is null)
        {
            _logger.LogWarning("Failed to process switch update request:");
            _logger.LogDebug("Switch entity not found:");

            return NotFound();
        }

        _logger.LogDebug("Switch entity found: Id=[{Id}]", knownSwitchEntity.Id);
        _logger.LogDebug("Updating switch details: ActualSwitchState=[{ActualSwitchState}]", request.ActualSwitchState);

        await _switchesRepository.UpdateSwitchAsync(
            knownSwitchEntity.Id,
            updateActualState: true,
            actualState: request.ActualSwitchState);

        _logger.LogDebug("Repository updated successfully:");
        _logger.LogInformation("Switch update request processed successfully:");

        return NoContent();
    }
}
