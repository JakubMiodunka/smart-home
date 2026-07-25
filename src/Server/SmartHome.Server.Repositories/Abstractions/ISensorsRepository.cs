using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Repositories.Enumerations;

namespace SmartHome.Server.Repositories.Abstractions;

/// <summary>
/// Defines interactions with repositories aggregating details about sensors functioning within the system.
/// </summary>
public interface ISensorsRepository
{
    /// <summary>
    /// Creates a new representation of a sensor within the repository.
    /// </summary>
    /// <param name="stationId">
    /// The unique identifier of the station that controls this sensor.
    /// </param>
    /// <param name="localId">
    /// The identifier of the sensor, unique only at the station level.
    /// </param>
    /// <param name="measurementType">
    /// Type of measurements taken by the sensor.
    /// Defines the nature of the data it collects (ex. temperature, humidity).
    /// </param>
    /// <returns>
    /// Sensor entity saved within the repository.
    /// </returns>
    Task<SensorEntity> CreateSensorAsync(long stationId, byte localId, MeasurementType measurementType);

    /// <summary>
    /// Retrieves single sensor from the repository basing on provided criteria.
    /// </summary>
    /// <param name="filterById">
    /// <see langword="true"/>, if filtering by sensor ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="id">
    /// Value of ID by which sensors shall be filtered.
    /// Ignored if value of <paramref name="filterById"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByStationId">
    /// <see langword="true"/>, if filtering by station ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="stationId">
    /// Value of station ID by which sensors shall be filtered.
    /// Ignored if value of <paramref name="filterByStationId"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByLocalId">
    /// <see langword="true"/>, if filtering by local ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="localId">
    /// Value of local ID by which sensors shall be filtered.
    /// Ignored if value of <paramref name="filterByLocalId"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Sensor entity that matches the provided criteria, or <see langword="null"/> reference if no match is found.
    /// </returns>
    Task<SensorEntity?> GetSingleSensorAsync(
        bool filterById = false,
        long? id = null,
        bool filterByStationId = false,
        long? stationId = null,
        bool filterByLocalId = false,
        byte? localId = null);

    /// <summary>
    /// Retrieves collection of sensors from the repository basing on provided criteria.
    /// </summary>
    /// <remarks>
    /// At the monent, there is no deed to use any filtering criteria here,
    /// but method is designed to be flexible to filtering in the future if needed.
    /// </remarks>
    /// <returns>
    /// Collection of sensor entities that matches the provided criteria.
    /// </returns>
    Task<SensorEntity[]> GetMultipleSensorsAsync();
}
