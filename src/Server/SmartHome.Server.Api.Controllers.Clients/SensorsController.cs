using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Controllers.Clients.Responses;
using SmartHome.Server.Api.Controllers.Common;
using SmartHome.Server.Features.Managers.Abstractions;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Api.Controllers.Clients;

/// <summary>
/// Controller dedicated to managing sensors.
/// </summary>
[ApiController]
[Route("api/clients/v1/sensors")]
public sealed class SensorsController : BaseController
{
    #region Properties
    private readonly ISensorsRepository _sensorsRepository;
    private readonly IStationsRepository _stationsRepository;
    private readonly ISensorManagerFactory _sensorManagerFactory;
    private readonly ILogger<SensorsController> _logger;
    #endregion

    #region Instationation
    /// <summary>
    /// Creates a new istnace of <see cref="SensorsController"/>.
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
    /// <param name="sensorManagerFactory">
    /// Factory which shall be used to create managers for performing operations on the sensors.
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
        ISensorManagerFactory sensorManagerFactory,
        ILogger<SensorsController> logger) : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(sensorsRepository, nameof(sensorsRepository));
        ArgumentNullException.ThrowIfNull(stationsRepository, nameof(stationsRepository));
        ArgumentNullException.ThrowIfNull(sensorManagerFactory, nameof(sensorManagerFactory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _sensorsRepository = sensorsRepository;
        _stationsRepository = stationsRepository;
        _sensorManagerFactory = sensorManagerFactory;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// Retrieves all sensors available in the repository.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of sensors.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetSensorsAsync()
    {
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning(
                "Request for getting collection of sensors rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request for getting collection of sensors: ClientIpAddress=[{ClientIpAddress}]",
            clientIpAddress);

        _logger.LogDebug("Searching for sensor entities:");

        SensorEntity[] allSensors = await _sensorsRepository.GetMultipleSensorsAsync();

        _logger.LogDebug("Sensor entities found: EntitiesReturned=[{EntitiesReturned}]", allSensors.Count());

        _logger.LogInformation(
            "Request processed successfully: ClientIpAddress=[{ClientIpAddress}, EntitiesReturned=[{EntitiesReturned}]",
            clientIpAddress,
            allSensors.Count());

        return Ok(allSensors);
    }

    /// <summary>
    /// Retrieves specified sensor.
    /// </summary>
    /// <param name="sensorId">
    /// The unique global identifier of the sensor.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of sensors.
    /// </returns>
    [HttpGet("{sensorId}")]
    public async Task<IActionResult> GetSensorAsync(long sensorId)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning(
                "Request for getting a sensor rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing request for getting a sensor: ClientIpAddress=[{ClientIpAddress}], SensorId=[{SensorId}]",
            clientIpAddress,
            sensorId);

        _logger.LogDebug("Searching for sensor entity: SensorId=[{SensorId}]", sensorId);

        SensorEntity? sensorEntity = await _sensorsRepository.GetSingleSensorAsync(filterById: true, id: sensorId);

        _logger.Log(
            sensorEntity is null ? LogLevel.Warning : LogLevel.Information,
            "Request processed successfully: Message=[{Message}], ClientIpAddress=[{ClientIpAddress}], SensorId=[{SensorId}]",
            sensorEntity is null ? "Sensor not found." : "Sensor found.",
            clientIpAddress,
            sensorId);

        return sensorEntity is null ? NotFound() : Ok(sensorEntity);
    }

    /// <summary>
    /// Attempts to take a measurement from the specified sensor.
    /// </summary>
    /// <param name="sensorId">
    /// The unique global identifier of the sensor,
    /// whihc is requested to take a measurement.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing value of measurement taken.
    /// </returns>
    [HttpGet("{sensorId}/measurement")]
    public async Task<IActionResult> GetMeasurementAsync(long sensorId, CancellationToken cancellationToken)
    {
        if (!TryGetRemoteIpAddress(out IPAddress? clientIpAddress))
        {
            _logger.LogWarning("Get measurement request rejected: Message=[{Message}]",
                "Failed to determine client IP address.");

            return BadRequest();
        }

        _logger.LogInformation(
            "Processing get measurement request: SensorId=[{SensorId}], ClientIpAddress=[{ClientIpAddress}]",
            sensorId,
            clientIpAddress);

        if (await _sensorsRepository.GetSingleSensorAsync(
                filterById: true,
                id: sensorId) is not SensorEntity sensorEntity)
        {
            _logger.LogWarning(
                "Failed to process get measurement request: " +
                "Message=[{Message}], SensorId=[{SensorId}], ClientIpAddress=[{ClientIpAddress}]",
                "Sensor not found.",
                sensorId,
                clientIpAddress);

            return NotFound();
        }

        if (await _stationsRepository.GetSingleStationAsync(
            filterById: true,
            id: sensorEntity.StationId) is not StationEntity parentStation)
        {
            _logger.LogError(
                "Failed to process get measurement request: " +
                "Message=[{Message}], SensorId=[{SensorId}], StationId=[{StationId}], ClientIpAddress=[{ClientIpAddress}]",
                "Parent station not found.",
                sensorEntity.Id,
                sensorEntity.StationId,
                clientIpAddress);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        _logger.LogDebug("Attempting to take a measurement: SensorId=[{SensorId}], StationId=[{StationId}]",
            sensorEntity.Id,
            parentStation.Id);

        ISensorManager sensorManager = _sensorManagerFactory.CreateFor(sensorEntity, parentStation);

        if (await sensorManager.TryGetMeasurementAsync(cancellationToken) is double measurementValue)
        {
            _logger.LogInformation(
                "Get measurement request processed successfully: " +
                "Message=[{Message}], SensorId=[{SensorId}], StationId=[{StationId}], MeasurementValue=[{MeasurementValue}]",
                "Measurement taken successfully.",
                sensorEntity.Id,
                parentStation.Id,
                measurementValue);

            return Ok(new GetMeasurementResponse(measurementValue));
        }

        _logger.LogInformation(
            "Get measurement request processed successfully: " +
            "Message=[{Message}], SensorId=[{SensorId}], StationId=[{StationId}]",
            "Sensor unreachable.",
            sensorEntity.Id,
            parentStation.Id);

        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
