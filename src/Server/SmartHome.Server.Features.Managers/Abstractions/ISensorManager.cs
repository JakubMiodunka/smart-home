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
    /// Indicator of success and value measured by the sensor.
    /// If attempt failed, the value will be <see langword="null"/>.
    /// </returns>
    Task<(bool success, double? value)> TryGetMeasurement(CancellationToken cancellationToken);
}
