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

    #region Sensor registration
    /// <summary>
    /// Registers provided sensor as new device within the system.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the sensor which shall be registered.
    /// </param>
    /// <param name="parentStationEntity">
    /// Entity of the station which controls the sensor.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    private async Task<IActionResult> RegisterSensorAsNewDeviceAsync(
        SensorRegistrationRequest request,
        StationEntity parentStationEntity)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentNullException.ThrowIfNull(parentStationEntity, nameof(parentStationEntity));

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

        SensorEntity sensorEntity = await _sensorsRepository.CreateSensorAsync(
            parentStationEntity.Id,
            request.SensorLocalId,
            request.MeasurementType);

        _logger.LogDebug("Repository updated successfully: SensorId=[{Id}]", sensorEntity.Id);

        _logger.LogInformation(
            "Sensor registration successful: " +
            "SensorId=[{Id}], StationIpAddress=[{StationIpAddress}]",
            sensorEntity.Id,
            parentStationEntity.IpAddress);

        var response = new SensorRegistrationResponse(sensorEntity.Id);
        return Ok(response);
    }

    /// <summary>
    /// Registers provided sensor as device already known within the system.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the sensor which shall be registered.
    /// </param>
    /// <param name="sensorEntity">
    /// Entity of the sensor which shall be registered.
    /// </param>
    /// <param name="parentStationEntity">
    /// Entity of the station which controls the sensor.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    private async Task<IActionResult> RegisterSensorAsKnownDeviceAsync(
        SensorRegistrationRequest request,
        SensorEntity sensorEntity,
        StationEntity parentStationEntity)
    {
        _logger.LogInformation(
                "Registering sensor as already known device: " +
                "SensorId=[{Id}], StationIpAddress=[{StationIpAddress}]",
                sensorEntity.Id,
                parentStationEntity.IpAddress);

        if (sensorEntity.MeasurementType != request.MeasurementType)
        {
            _logger.LogWarning(
                "Failed to process sensor registration request: Message=[{Message}], StationId=[{StationId}]," +
                "LocalId=[{LocalId}], ExpectedMeasurementType=[{ExpectedMeasurementType}]," +
                "ActualMeasurementType=[{ActualMeasurementType}]",
                "Reported measurement type does not match existing sensor.",
                parentStationEntity.Id,
                sensorEntity.Id,
                sensorEntity.MeasurementType,
                request.MeasurementType);

            return Conflict();
        }

        _logger.LogInformation(
            "Sensor registration successful: " +
            "SensorId=[{Id}], StationIpAddress=[{StationIpAddress}]",
            sensorEntity.Id,
            parentStationEntity.IpAddress);

        var response = new SensorRegistrationResponse(sensorEntity.Id);
        return Ok(response);
    }

    /// <summary>
    /// Registers a sensor within the system using details provided in request body.
    /// </summary>
    /// <param name="request">
    /// Data transfer object (DTO) containing details about the sensor which shall be registered.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the performed operation.
    /// </returns>
    [HttpPut]
    public async Task<IActionResult> RegisterSensorAsync([FromBody] SensorRegistrationRequest request)
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
            localId: request.SensorLocalId) is SensorEntity sensorEntity)
        {
            _logger.LogDebug("Sensor entity found: SensorId=[{Id}]", sensorEntity.Id);

            return await RegisterSensorAsKnownDeviceAsync(request, sensorEntity, parentStationEntity);
        }

        _logger.LogDebug(
            "Sensor entity not found: StationId=[{StationId}], LocalId=[{LocalId}]",
            parentStationEntity.Id,
            request.SensorLocalId);

        return await RegisterSensorAsNewDeviceAsync(request, parentStationEntity);
    }
    #endregion
}
