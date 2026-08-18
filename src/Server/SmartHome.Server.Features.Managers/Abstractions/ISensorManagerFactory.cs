using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Features.Managers.Abstractions;

/// <summary>
/// Factory for creating managers that controls sensors.
/// </summary>
public interface ISensorManagerFactory
{
    /// <summary>
    /// Creates manager for the sensor.
    /// </summary>
    /// <param name="sensorEntity">
    /// Entity of sensor which shall be controlled by created manager.
    /// </param>
    /// <param name="parentStation">
    /// Entity of station, that controls the specified sensor.
    /// </param>
    /// <returns>
    /// An <see cref="ISensorManager"/> instance that allows performing operations on the sensor.
    /// </returns>
    ISensorManager CreateFor(SensorEntity sensorEntity, StationEntity parentStation);
}
