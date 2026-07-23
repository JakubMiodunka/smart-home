namespace SmartHome.Server.Api.Controllers.Firmware.Responses;

/// <summary>
/// Data transfer object (DTO) representing a response after successful sensor registration.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="SensorId">
/// The unique global identifier assigned to registered sensor.
/// </param>
internal sealed record SensorRegistrationResponse(long SensorId);
