using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Api.Controllers.Firmware.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to register a switch within the system.
/// </summary>
/// <param name="SwitchLocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
public sealed record SwitchRegistrationRequest([Range(1, byte.MaxValue)] byte SwitchLocalId);
