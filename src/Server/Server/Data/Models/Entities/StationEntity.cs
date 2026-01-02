using SmartHome.Server.Data.Models.Dtos;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;

/// <summary>
/// Data transfer object (DTO) representing the general details of a station reqistered within the system.
/// Used for data exchange between the server and the API clients.
/// </summary>
/// <param name="MacAddress">
/// The unique physical MAC address of the station, used as a primary identifier in the network.
/// </param>

/// <summary>
/// Entity representing the general details of a station reqistered within the system.
/// Used for data exchange between the server and database.
/// </summary>
/// <param name="Id">
/// Identifier unique to represented station.
/// </param>
/// <param name="MacAddress">
/// The unique physical MAC address of the station, used as a primary identifier in the network.
/// </param>
/// <param name="IpAddress">
/// IP address assigned to the station within the network.
/// Null reference shall be used when address is unknown.
/// </param>
public sealed record StationEntity(long Id, PhysicalAddress MacAddress, IPAddress? IpAddress)
{
    public StationDto ToDto() =>
        new StationDto(MacAddress);
}
