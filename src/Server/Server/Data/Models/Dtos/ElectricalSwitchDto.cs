using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Dtos;

/// <summary>
/// Data transfer object (DTO) representing the details of an electrical switch functioning within the system.
/// Used for data exchange between the server and API clients.
/// </summary>
/// <param name="StationMacAddress">
/// The MAC address of the parent station this switch belongs to.
/// </param>
/// <param name="LocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
/// <param name="isClosed">
/// The state of the electrical switch. <see langword="true"/> if the circuit is closed 
/// and current is flowing, <see langword="false"/> otherwise.
/// Use <see langword="null"/> when the state of the switch is unknown.
/// </param>
public sealed record ElectricalSwitchDto(PhysicalAddress StationMacAddress, byte LocalId, bool? IsClosed);
