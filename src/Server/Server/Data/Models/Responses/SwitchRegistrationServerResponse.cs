namespace SmartHome.Server.Data.Models.Responses;

/// <summary>
/// Data transfer object (DTO) representing a response after successful switch registration.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="SwitchId">
/// The unique global identifier for the electrical switch.
/// </param>
/// <param name="ExpectedSwitchState">
/// Desired state of the switch after its registration.
/// <see langword="true"/> if the circuit shall be closed 
/// and current shall flow; <see langword="false"/> otherwise.
/// </param>
public sealed record SwitchRegistrationServerResponse(long SwitchId, bool ExpectedSwitchState);
