namespace SmartHome.Server.Api.Controllers.Firmware.Responses;

/// <summary>
/// Data transfer object (DTO) representing a response after successful switch registration.
/// </summary>
/// <param name="SwitchId">
/// The unique global identifier assigned to registered switch.
/// </param>
/// <param name="ExpectedSwitchState">
/// Desired state of the switch after its registration.
/// <see langword="true"/> if the circuit shall be closed 
/// and current shall flow; <see langword="false"/> otherwise.
/// </param>
internal sealed record SwitchRegistrationResponse(long SwitchId, bool ExpectedSwitchState);
