using SmartHome.Server.Repositories.Enumerations;

namespace SmartHome.Server.Repositories.Entities;

/// <summary>
/// Entity representing the details of a sensor functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <param name="Id">
/// The unique global identifier for the sensor.
/// </param>
/// <param name="StationId">
/// The unique identifier of the station that controls this sensor.
/// </param>
/// <param name="LocalId">
/// The identifier of the sensor, unique only at the station level.
/// </param>
/// <param name="MeasurementType">
/// Type of measurements taken by the sensor.
/// Defines the nature of the data it collects (ex. temperature, humidity).
/// </param>
public sealed record SensorEntity(
    long Id,
    long StationId,
    byte LocalId,
    MeasurementType MeasurementType);
