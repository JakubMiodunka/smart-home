namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request created by the server
/// to update properties of electrical switch on station site.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station.
/// </remarks>
/// <param name="ExpectedSwitchState">
/// Desired state of the switch.
/// <see langword="true"/> if the circuit shall be closed 
/// and current shall flow; <see langword="false"/> otherwise.
/// </param>
public sealed record SwitchUpdateServerRequest(bool ExpectedSwitchState);
