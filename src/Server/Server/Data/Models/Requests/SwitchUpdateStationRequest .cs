using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to update
/// details related to particular electrical switch on the server side.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="ActualSwitchState">
/// Current state of the electrical switch.
/// <see langword="true"/> if the circuit is closed and current is flowing;
/// <see langword="false"/> otherwise.
/// </param>
public sealed record SwitchUpdateStationRequest([Required] bool ActualSwitchState);
