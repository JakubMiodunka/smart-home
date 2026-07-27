using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Controllers.Common;
using SmartHome.Server.Api.Controllers.Firmware.Requests;
using SmartHome.Server.Api.Controllers.Firmware.Responses;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Api.Controllers.Firmware;

// TODO: Add unit tests.
/// <summary>
/// Controller dedicated to sensor management.
/// </summary>
[Route("api/firmware/v1/sensors")]
public sealed class SensorsController : BaseController
{
    #region Properties
    private readonly ISensorsRepository _sensorsRepository;
    private readonly IStationsRepository _stationsRepository;
    private readonly ILogger<SensorsController> _logger;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates an new instanve of <see cref="SensorsController"/>.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <param name="sensorsRepository">
    /// Sensors repository which shall be used by this controller.
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
    public SensorsController(
        IHttpContextAccessor httpContextAccessor,
        ISensorsRepository sensorsRepository,
        IStationsRepository stationsRepository,
        ILogger<SensorsController> logger)
        : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(sensorsRepository, nameof(sensorsRepository));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _sensorsRepository = sensorsRepository;
        _stationsRepository = stationsRepository;
        _logger = logger;
    }
    #endregion

    // TODO: Add check if repotted measurement type is same as saved in DB
    /// <summary>
    /// Registers a sensor within the system using details provided in request body.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the sensor which shall be registered within the system.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> RegisterSensor([FromBody] SensorRegistrationRequest request)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? stationIpAddress))
        {
            _logger.LogWarning(
                "Sensor registration request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing sensor registration request: StationIpAddress=[{StationIpAddress}], SensorLocalId=[{SensorLocalId}]",
            stationIpAddress,
            request.SensorLocalId);

        _logger.LogDebug(
            "Searching for parent station entity: StationIpAddress=[{StationIpAddress}]",
            stationIpAddress);

        if (await _stationsRepository.GetSingleStationAsync(
                filterByIpAddress: true,
                ipAddress: stationIpAddress) is not StationEntity parentStationEntity)
        {
            _logger.LogWarning(
                "Failed to process sensor registration request: Message=[{Message}], StationIpAddress=[{StationIpAddress}]",
                "Parent station entity not found.",
                stationIpAddress);

            return NotFound();
        }

        _logger.LogDebug("Parent station entity found: StationId=[{Id}]", parentStationEntity.Id);

        _logger.LogDebug(
            "Searching for sensor entity: StationId=[{StationId}], LocalId=[{LocalId}]",
            parentStationEntity.Id,
            request.SensorLocalId);

        if (await _sensorsRepository.GetSingleSensorAsync(
                filterByStationId: true,
                stationId: parentStationEntity.Id,
                filterByLocalId: true,
                localId: request.SensorLocalId) is not SensorEntity sensorEntity)
        {
            _logger.LogDebug(
                "Sensor entity not found: StationId=[{StationId}], LocalId=[{LocalId}]",
                parentStationEntity.Id,
                request.SensorLocalId);

            _logger.LogInformation(
                "Registering sensor as a new device within the system: " +
                "StationId=[{StationId}], StationIpAddress=[{StationIpAddress}], LocalId=[{LocalId}]",
                parentStationEntity.Id,
                parentStationEntity.IpAddress,
                request.SensorLocalId);

            _logger.LogDebug(
                "Creating new sensor entity:" +
                " StationId=[{StationId}], LocalId=[{LocalId}], MeasurementType=[{MeasurementType}]",
                parentStationEntity.Id,
                request.SensorLocalId,
                request.MeasurementType);

            sensorEntity = await _sensorsRepository.CreateSensorAsync(
                parentStationEntity.Id,
                request.SensorLocalId,
                request.MeasurementType);

            _logger.LogDebug("Repository updated successfully: SensorId=[{Id}]", sensorEntity.Id);
        }
        else
        {
            _logger.LogDebug("Sensor entity found: SensorId=[{Id}]", sensorEntity.Id);

            _logger.LogInformation(
                "Registering sensor as already known device: " +
                "SensorId=[{Id}], StationIpAddress=[{StationIpAddress}]",
                sensorEntity.Id,
                parentStationEntity.IpAddress);
        }

        _logger.LogInformation(
            "Sensor registration successful: " +
            "SensorId=[{Id}], StationIpAddress=[{StationIpAddress}]",
            sensorEntity.Id,
            parentStationEntity.IpAddress);

        var response = new SensorRegistrationResponse(sensorEntity.Id);
        return Ok(response);
    }
}
