namespace SmartHome.Server.Api.Controllers.Clients.Responses;

/// <summary>
/// Data transfer object (DTO) representing a response on successfull
/// measurement retrieval request.
/// </summary>
/// <param name="MeasurementValue">
/// Value of the measurement retrieved from the sensor.
/// </param>
internal sealed record GetMeasurementResponse(double MeasurementValue);
