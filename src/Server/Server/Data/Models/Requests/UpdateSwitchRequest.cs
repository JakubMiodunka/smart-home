using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to update
/// date related to particular electrical switch on the server side.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="StationMacAddress">
/// The MAC address of the station that controls this switch.
/// </param>
/// <param name="SwitchLocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
/// <param name="SwitchState">
/// Current state of the electrical switch.
/// <see langword="true"/> if the circuit is closed and current is flowing;
/// <see langword="false"/> otherwise.
/// </param>
public sealed record UpdateSwitchRequest(
    [Required] PhysicalAddress StationMacAddress,
    [Range(1, byte.MaxValue)] byte SwitchLocalId,
    [Required] bool SwitchState);
