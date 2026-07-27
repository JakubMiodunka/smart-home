using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Features.Managers.Requests;

/// <summary>
/// Data transfer object (DTO) representing a response created
/// by the station firmware on the server request to take a measurement from a sensor.
/// </summary>
/// <param name="MeasurementValue">
/// Value of the measurement taken by the sensor.
/// </param>
internal sealed record GetMeasurementResponse([Required] double MeasurementValue);
