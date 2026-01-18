using SmartHome.Server.Data.Models.Dtos;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;

/// <summary>
/// Entity representing the general details of a station functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <param name="Id">
/// The unique global identifier for the station.
/// </param>
/// <param name="MacAddress">
/// The unique physical MAC address of the station, used as a primary identifier in the network.
/// </param>
/// <param name="IpAddress">
/// The IP address assigned to the station within the network. 
/// A <see langword="null"/> value indicates the address is unknown.
/// </param>
public sealed record StationEntity(long Id, PhysicalAddress MacAddress, IPAddress? IpAddress);

/// <summary>
/// Extensions for <see cref="StationEntity"/>
/// </summary>
public static class StationEntityExtensions
{
    /// <summary>
    /// Creates a Data Transfer Object (DTO) corresponding to this entity.
    /// </summary>
    /// <returns>
    /// Data Transfer Object (DTO) corresponding to this entity.
    /// </returns>

    public static StationDto ToDto(this StationEntity stationEntity)
    {
        ArgumentNullException.ThrowIfNull(stationEntity, nameof(stationEntity));

        return new StationDto(
            Id: stationEntity.Id,
            MacAddress: stationEntity.MacAddress);
    }
}
