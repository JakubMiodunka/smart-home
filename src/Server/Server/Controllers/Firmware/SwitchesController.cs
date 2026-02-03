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
[Route("firmware-api/v1/switches")]
public class SwitchesController : FirmwareController
{
    #region Constraints
    private const bool DefaultSwitchState = false;
    #endregion

    #region Properties
    private readonly ISwitchesRepository _switchesRepository;
    private readonly IStationsRepository _stationsRepository;
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
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchesController(
        IHttpContextAccessor httpContextAccessor,
        ISwitchesRepository switchesRepository,
        IStationsRepository stationsRepository)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));

        _switchesRepository = switchesRepository;
        _stationsRepository = stationsRepository;
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
        if (!TryGetRemoteIpAddress(out IPAddress? remoteIpAddress))
        {
            return BadRequest("Unable to determine remote IP address.");
        }

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.StationMacAddress,
                filterByIpAddress: true,
                ipAddress: remoteIpAddress);

        if (parentStationEntity is null)
        {
            return NotFound("Switch parent station not found.");
        }

        SwitchEntity? knownSwitchEntity = 
            await _switchesRepository.GetSingleSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SwitchLocalId);

        if (knownSwitchEntity is null)
        {
            await _switchesRepository.CreateSwitchAsync(
                parentStationEntity.Id,
                request.SwitchLocalId,
                DefaultSwitchState,
                actualState: null);

            return Ok(new SwitchRegistrationResponse(DefaultSwitchState));
        }

        return Ok(new SwitchRegistrationResponse(knownSwitchEntity.ExpectedState));
    }

    /// <summary>
    /// Updates the actual state of an electrical switch according to details provided in request body.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the switch which shall be updated along with its current state.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPatch("state")]
    public async Task<IActionResult> UpdateSwitchState([FromBody] UpdateSwitchStateRequest request)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? remoteIpAddress))
        {
            return BadRequest("Unable to determine remote IP address.");
        }

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByMacAddress: true,
                macAddress: request.StationMacAddress,
                filterByIpAddress: true,
                ipAddress: remoteIpAddress);

        if (parentStationEntity is null)
        {
            return NotFound("Switch parent station not found.");
        }

        SwitchEntity? knownSwitchEntity =
            await _switchesRepository.GetSingleSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SwitchLocalId);

        if (knownSwitchEntity is null)
        {
            return NotFound("Switch not found.");
        }

        await _switchesRepository.UpdateSwitchAsync(
            knownSwitchEntity.Id,
            updateActualState: true,
            actualState: request.SwitchState);

        return NoContent();
    }
}
