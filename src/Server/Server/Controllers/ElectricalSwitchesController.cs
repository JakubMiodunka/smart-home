using Microsoft.AspNetCore.Mvc;
using SmartHome.Server.Data.Models.Dtos;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Controllers;

/// TODO Add manager to be able to change electrical switch state.
/// <summary>
/// Controller dedicated to managing electrical switches.
/// </summary>
[ApiController]
[Route("api/v1/electrical-switches")]
public class ElectricalSwitchesController : ControllerBase
{
    #region Properties
    private readonly IElectricalSwitchesRepository _electricalSwitchesRepository;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new controller instance.
    /// </summary>
    /// <param name="electricalSwitchesRepository">
    /// Stations repository which shall be used by this controller.
    /// </param>
    /// <param name="stationsRepository">
    /// Electrical switches repository which shall be used by this controller.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public ElectricalSwitchesController(IElectricalSwitchesRepository electricalSwitchesRepository)
    {
        ArgumentNullException.ThrowIfNull(electricalSwitchesRepository, nameof(electricalSwitchesRepository));

        _electricalSwitchesRepository = electricalSwitchesRepository;
    }
    #endregion

    /// TODO Return ex. bad request if station with is specified in request body does not exist in database.
    /// <summary>
    /// Registers an electrical switch within the system using details provided in request body.
    /// </summary>
    /// <param name="electricalSwitchDto">
    /// Data transfer object (DTO) containing details about electrical switch which shall be registered within the system.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> RegisterElectricalSwitch([FromBody] ElectricalSwitchDto electricalSwitchDto)
    {
        ElectricalSwitchEntity? knownElectricalSwitchEntity = 
            await _electricalSwitchesRepository.GetSingleElectricalSwitchAsync(
                filterByStationId: true,
                stationId: electricalSwitchDto.StationId,
                filterByLocalId: true,
                localId: electricalSwitchDto.LocalId);

        if (knownElectricalSwitchEntity is null)
        {
            ElectricalSwitchEntity createdElectricalSwitchEntity =
                await _electricalSwitchesRepository.CreateElectricalSwitchAsync(
                    electricalSwitchDto.StationId,
                    electricalSwitchDto.LocalId,
                    electricalSwitchDto.IsClosed);

            return CreatedAtAction(
                nameof(GetElectricalSwitch),
                new { id = createdElectricalSwitchEntity.Id },
                createdElectricalSwitchEntity.ToDto());
        }

        if (knownElectricalSwitchEntity.IsClosed == electricalSwitchDto.IsClosed)
        {
            return Ok(knownElectricalSwitchEntity.ToDto());
        }

        ElectricalSwitchEntity? updatedElectricalSwitchEntity =
            await _electricalSwitchesRepository.UpdateElectricalSwitchAsync(
                knownElectricalSwitchEntity.Id,
                updateState: true,
                isClosed: electricalSwitchDto.IsClosed);

        return Ok(updatedElectricalSwitchEntity?.ToDto());
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
        ElectricalSwitchEntity? electricalSwitchEntity =
            await _electricalSwitchesRepository.GetSingleElectricalSwitchAsync(filterById: true, id: id);

        return electricalSwitchEntity is null ? NotFound() : Ok(electricalSwitchEntity.ToDto());
    }
}
