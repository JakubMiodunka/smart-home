namespace SmartHome.Server.Api.Controllers.Firmware.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to update
/// details related to particular electrical switch on the server side.
/// </summary>
/// <param name="ActualSwitchState">
/// Current state of the electrical switch.
/// <see langword="true"/> if the circuit is closed and current is flowing;
/// <see langword="false"/> otherwise.
/// </param>
public sealed record SwitchUpdateRequest(bool ActualSwitchState);
