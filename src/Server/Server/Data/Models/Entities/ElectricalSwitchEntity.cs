using SmartHome.Server.Data.Models.Dtos;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;


/// <summary>
/// Entity representing the details of an electrical switch functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <param name="Id">
/// The unique global identifier for the electrical switch.
/// </param>
/// <param name="StationId">
/// The unique identifier of the station that controls this switch.
/// </param>
/// <param name="LocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
/// <param name="IsClosed">
/// The state of the electrical switch. <see langword="true"/> if the circuit is closed 
/// and current is flowing; <see langword="false"/> otherwise. 
/// A <see langword="null"/> value indicates the state is unknown.
/// </param>
public sealed record ElectricalSwitchEntity(long Id, long StationId, byte LocalId, bool? IsClosed)
{
    public ElectricalSwitchDto ToDto(PhysicalAddress stationMacAddress) =>
        new ElectricalSwitchDto(stationMacAddress, LocalId, IsClosed);
}
