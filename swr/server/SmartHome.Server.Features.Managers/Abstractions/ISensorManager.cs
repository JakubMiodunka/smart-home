namespace SmartHome.Server.Features.Managers.Abstractions;

/// <summary>
/// Controlls specific sensor on a remote station.
/// </summary>
public interface ISensorManager
{
    /// <summary>
    /// Attempts to take a measurement from the controlled sensor.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// Value of measuremnt taken by the sensor if attempt was successful,
    /// <see langword="null"/> otherwise.
    /// </returns>
    Task<double?> TryGetMeasurementAsync(CancellationToken cancellationToken);
}
