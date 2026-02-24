using SmartHome.Server.Services.Processors;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;

/// TODO: Concider adding timestamp of entity creation to avoid potential issues with processing outdated data.
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
/// A <see langword="null"/> value indicates that station is offline and address is unknown.
/// </param>
/// <param name="LastHeartbeat">
/// Timestamp of the last heartbeat signal received from the station.
/// </param>
public sealed record StationEntity(long Id, PhysicalAddress MacAddress, IPAddress? IpAddress, DateTimeOffset LastHeartbeat);

/// <summary>
/// Defines logic for <see cref="StationEntity"/> that are not persisted directly in the database.
/// </summary>
public static class StationEntityExtensions
{
    /// <summary>
    /// Determines whether the station is considered online.
    /// </summary>
    /// <remarks>
    /// A station is defined as online only if it has an assigned IP address. 
    /// The <see cref="HeartbeatMonitoringServiceProcessor"> is responsible 
    /// for nullifying the IP address if a heartbeat timeout occurs.
    /// </remarks>
    /// <param name="stationEntity">T
    /// Station entity to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the station is considered online.
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsOnline(this StationEntity stationEntity) =>
        stationEntity.IpAddress is not null;
}