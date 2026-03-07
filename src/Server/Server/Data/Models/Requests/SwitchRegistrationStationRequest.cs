using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to register a switch within the system.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="SwitchLocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
public sealed record SwitchRegistrationStationRequest([Range(1, byte.MaxValue)] byte SwitchLocalId);
