using Microsoft.Extensions.Logging;
using SmartHome.Server.Api.Clients;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Features.Managers.Abstractions;
using SmartHome.Server.Features.Managers.Responses;
using SmartHome.Server.Repositories.Entities;
using System.Net;

namespace SmartHome.Server.Features.Managers;

/// <inheritdoc cref="ISensorManager"/>
internal sealed class SensorManager : FeatureManager, ISensorManager
{
    #region Properties
    // TODO: Move this value to some configuration file.
    private static TimeSpan StationApiClientTimeout =>
        TimeSpan.FromMilliseconds(5000);
    private readonly ILogger<SensorManager> _logger;

    public SensorEntity ManagedSensor { get; private set; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SensorManager"/>.
    /// </summary>
    /// <param name="managedSensor">
    /// Entity of the sensor managed by created manager instance.
    /// </param>
    /// <param name="parentStation">
    /// Parent station of the managed sensor.
    /// </param>
    /// <param name="stationApiClientsFactory">
    /// Factory of station API clients, which shall be used to obtain clients
    /// capable of communicating with station associated with the managed sensor.
    /// </param>
    /// <param name="logger">
    /// Logger which shall be used by this manager.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    public SensorManager(
        SensorEntity managedSensor,
        StationEntity parentStation,
        IStationApiClientFactory stationApiClientsFactory,
        ILogger<SensorManager> logger)
        : base(parentStation, stationApiClientsFactory, StationApiClientTimeout)
    {
        ArgumentNullException.ThrowIfNull(managedSensor, nameof(managedSensor));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (managedSensor.StationId != ParentStation.Id)
        {
            throw new ArgumentException(
                "The station entity shall be the parent station of the sensor entity: " +
                $"SensorStationId=[{managedSensor.StationId}], StationId=[{parentStation.Id}]",
                nameof(parentStation));
        }

        _logger = logger;
        ManagedSensor = managedSensor;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Determines the URL of API endpoint which controls the sensor.
    /// </summary>
    /// <returns>
    /// Absolute URL of API endpoint which controls the sensor,
    /// or <see langword="null"/> if endpoint is considered as unreachable.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when generation of sensor URL is not supported sensor parent station API version.
    /// </exception>
    private Uri? GetSensorUrl()
    {
        if (GetStationBaseApiUrl() is not Uri baseStationApiUrl) return null;

        Uri sensorApiEndpoint = ParentStation.ApiVersion switch
        {
            1 => new Uri($"sensors/{ManagedSensor.LocalId}", UriKind.Relative),
            _ => throw new NotSupportedException($"Station API version not supported: ApiVersion=[{ParentStation.ApiVersion}]")
        };

        return new Uri(baseStationApiUrl, sensorApiEndpoint);
    }

    /// <inheritdoc cref="ISwitchManager"/>
    /// <summary>
    /// Sends a command to station associated with managed switch to take a measurement.
    /// </summary>
    public async Task<double?> TryGetMeasurementAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to take a measurement: SensorId=[{SensorId}], StationId=[{StationId}]",
            ManagedSensor.Id,
            ParentStation.Id);
    
        if (GetSensorUrl() is not Uri endpointUrl)
        {
            _logger.LogWarning(
                "Sensor is unreachable: SensorId=[{SensorId}], StationId=[{StationId}]",
                ManagedSensor.Id,
                ParentStation.Id);
    
            return null;
        }
    
        StationApiResponse<GetMeasurementResponse>? response = 
            await StationApiClient.SendRequestAsync<GetMeasurementResponse>(
                endpointUrl,
                HttpMethod.Get,
                null,
                cancellationToken);

        /*
         * TODO: That kind of response validation is a common task amogn managers and generates too much boilerplate code.
         * Rework needed.
         */
        if (response is null)
        {
            _logger.LogWarning(
                "Attempt to take a measurement failed: Message=[{Message}], SensorId=[{SensorId}], StationId=[{StationId}]",
                "Failed to send a request",
                ManagedSensor.Id,
                ParentStation.Id);

            return null;
        }

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            _logger.LogError(
                "Attempt to take a measurement failed: Message=[{Message}], SensorId=[{SensorId}], StationId=[{StationId}]",
                "Unexpected HTTP status received.",
                ManagedSensor.Id,
                ParentStation.Id);

            return null;
        }

        _logger.LogInformation("Attempt to take a measurement successful: " +
            "SensorId=[{SensorId}], StationId=[{StationId}], Value=[{Value}]",
            ManagedSensor.Id,
            ParentStation.Id,
            response.Body.MeasurementValue);

        return response.Body.MeasurementValue;
    }
    #endregion
}
