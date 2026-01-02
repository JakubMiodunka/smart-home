using SmartHome.Server.Data.Models.Dtos;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;


/// <summary>
/// Entity representing the details of an electrical switch registered within the system.
/// Used for data exchange between the server and database.
/// </summary>
/// <param name="Id">
/// Identifier unique to represented electrical switch.
/// </param>
/// <param name="StationId">
/// Identifier uniwue to station, which controlls the electrical switch.
/// </param>
/// <param name="LocalId">
/// Identifier of the switch, unique at the station level.
/// </param>
/// <param name="IsClosed">
/// State of the electrical switch - true if the cuirquit is closed and current is flowing, false otherwise.
/// Null value shall be used when state of the swith is unknown.
/// </param>
public sealed record ElectricalSwitchEntity(long Id, long StationId, byte LocalId, bool? IsClosed)
{
    public ElectricalSwitchDto ToDto(PhysicalAddress stationMacAddress) =>
        new ElectricalSwitchDto(stationMacAddress, LocalId, IsClosed);
}
