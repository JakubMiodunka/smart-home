using Microsoft.AspNetCore.Mvc;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers.Factories;
using System.Net;

namespace SmartHome.Server.Controllers;

/// <summary>
/// Controller dedicated to managing electrical switches.
/// </summary>
[ApiController]
[Route("api/v1/electrical-switches")]
public class ElectricalSwitchesController : SmartHomeController
{
    #region Properties
    private readonly IElectricalSwitchesRepository _switchesRepository;
    private readonly IElectricalSwitchManagerFactory _switchManagersFactory;
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
    /// <param name="switchManagerFactory">
    /// Factory which shall be used to obtain managers required to to control electrical switches.
    /// </param>
    /// <param name="stationsRepository">
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public ElectricalSwitchesController(
        IHttpContextAccessor httpContextAccessor,
        IElectricalSwitchesRepository switchesRepository,
        IElectricalSwitchManagerFactory switchManagerFactory,
        IStationsRepository stationsRepository)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(switchesRepository, nameof(switchesRepository));
        ArgumentNullException.ThrowIfNull(switchManagerFactory, nameof(switchManagerFactory));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));

        _switchesRepository = switchesRepository;
        _switchManagersFactory = switchManagerFactory;
        _stationsRepository = stationsRepository;
    }
    #endregion

    /// <summary>
    /// Registers an electrical switch within the system using details provided in request body.
    /// </summary>
    /// <param name="switchDto">
    /// Data transfer object (DTO) containing details about electrical switch which shall be registered within the system.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> RegisterElectricalSwitch([FromBody] ElectricalSwitchDto switchDto)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? remoteIpAddress))
        {
            return BadRequest("Unable to determine remote IP address.");
        }

        StationEntity? parentStationEntity =
            await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: remoteIpAddress);

        if (parentStationEntity is null)
        {
            return BadRequest("Unable to determine electrical switch parent station.");
        }

        ElectricalSwitchEntity? knownSwitchEntity = 
            await _switchesRepository.GetSingleElectricalSwitchAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: switchDto.LocalId);

        if (knownSwitchEntity is null)
        {
            ElectricalSwitchEntity createdSwitchEntity =
                await _switchesRepository.CreateElectricalSwitchAsync(
                    parentStationEntity.Id,
                    switchDto.LocalId,
                    switchDto.IsClosed);

            return CreatedAtAction(
                nameof(GetElectricalSwitch),
                new { id = createdSwitchEntity.Id },
                createdSwitchEntity.ToDto());
        }

        if (knownSwitchEntity.IsClosed == switchDto.IsClosed)
        {
            return Ok(knownSwitchEntity.ToDto());
        }

        ElectricalSwitchEntity? updatedSwitchEntity =
            await _switchesRepository.UpdateElectricalSwitchAsync(
                knownSwitchEntity.Id,
                updateState: true,
                isClosed: switchDto.IsClosed);

        return Ok(updatedSwitchEntity?.ToDto());
    }

    /// <summary>
    /// Retrieves details about a specific electrical switch.
    /// </summary>
    /// <param name="id">
    /// Identifier of the electrical switch to be retrieved.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpGet("{id:long:min(1)}")]
    public async Task<IActionResult> GetElectricalSwitch(long id)
    {
        ElectricalSwitchEntity? switchEntity =
            await _switchesRepository.GetSingleElectricalSwitchAsync(filterById: true, id: id);

        return switchEntity is null ? NotFound() : Ok(switchEntity.ToDto());
    }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// TODO: Add docstring.
    /// TODO: Add tests to this method.
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="shallBeClosed"></param>
    /// <returns></returns>
    [HttpPut("{id:long:min(1)}/state/{shallBeClosed:bool}")]
    public async Task<IActionResult> ChangeStateOfElectricalSwitch(long id, bool shallBeClosed)
    {
        ElectricalSwitchEntity? switchEntity = 
            await _switchesRepository.GetSingleElectricalSwitchAsync(filterById: true, id: id);

        if (switchEntity is null)
        {
            return NotFound();
        }

        if (_switchManagersFactory.CreateFor(switchEntity).TryChangeState(shallBeClosed))
        {
            ElectricalSwitchEntity? updatedSwitchEntity = 
                await _switchesRepository.UpdateElectricalSwitchAsync(id, updateState: true, isClosed: shallBeClosed);

            return Ok(updatedSwitchEntity?.ToDto());
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, switchEntity);
    }
}
