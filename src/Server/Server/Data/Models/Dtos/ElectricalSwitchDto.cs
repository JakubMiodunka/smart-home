using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Dtos;

/// <summary>
/// Data transfer object (DTO) representing the details of an electrical switch registered within the system.
/// Used for data exchange between the server and API clients.
/// </summary>
/// <param name="StationMacAddress">
/// The MAC address of the parent station this switch belongs to.
/// </param>
/// <param name="LocalId">
/// Identifier of the switch, unique at the station level.
/// </param>
/// <param name="IsClosed">
/// State of the electrical switch - true if the cuirquit is closed and current is flowing, false otherwise.
/// Null value shall be used when state of the swith is unknown.
/// </param>
public sealed record ElectricalSwitchDto(PhysicalAddress StationMacAddress, byte LocalId, bool? IsClosed);
