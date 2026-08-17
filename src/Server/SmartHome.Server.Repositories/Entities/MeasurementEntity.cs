namespace SmartHome.Server.Repositories.Entities;

// TODO: Not used yet - will be used to store scheduled measurements taken by the sensors.
/// <summary>
/// Represents a measurement taken by a sensor.
/// </summary>
/// <param name="Id">
/// The unique global identifier for the measurement.
/// </param>
/// <param name="SensorId">
/// Identifier of the sensor that recorded the measurement.
/// </param>
/// <param name="Value">
/// The measured value.
/// </param>
/// <param name="Timestamp">
/// Timestamp indicating when the measurement was recorded.
/// </param>
public sealed record MeasurementEntity(
    long Id,
    long SensorId,
    double Value,
    DateTimeOffset Timestamp);
