namespace SmartHome.Server.Api.Controllers.Firmware.Responses;

/// <summary>
/// Data transfer object (DTO) representing a response after successful sensor registration.
/// </summary>
/// <param name="SensorId">
/// The unique global identifier assigned to registered sensor.
/// </param>
internal sealed record SensorRegistrationResponse(long SensorId);
