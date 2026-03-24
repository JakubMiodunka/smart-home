using SmartHome.Server.Services.Processors;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;

/// <summary>
/// Entity representing the general details of a station functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <remarks>
/// Although <see langword="ushort"/> would perfectly fit the network port range,
/// <see langword="int"/> is used here for consistency with the .NET ecosystem 
/// (ex. <see cref="IPEndPoint.Port"/>) and to ensure compatibility with 
/// SQL Server, which lacks a native unsigned 16-bit integer type. 
/// This avoids unnecessary type casting and simplifies data mapping.
/// </remarks>
/// <param name="Id">
/// The unique global identifier for the station.
/// </param>
/// <param name="MacAddress">
/// The unique physical MAC address of the station, used as a primary identifier in the network.
/// </param>
/// <param name="IpAddress">
/// The IP address assigned to the station within the network. 
/// Set to <see langword="null"/> if station is offline and address is unknown.
/// </param>
/// <param name="ApiPort">
/// The network port on which the station's control service is listening.
/// Shall be in range from <see cref="IPEndPoint.MinPort"/> to <see cref="IPEndPoint.MaxPort"/>.
/// As this record serves purely as a data container, range validation is enforced at the database level via constraints.
/// Set to <see langword="null"/> if station is offline and port is unknown.
/// </param>
/// <param name="ApiVersion">
/// Version of the API exposed by the station.
/// Set to <see langword="null"/> if station is offline and its API version is unknown.
/// </param>
/// <param name="LastHeartbeat">
/// Timestamp of the last heartbeat signal received from the station.
/// </param>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when the value of at least one argument is outside its valid range.
/// </exception>
public sealed record StationEntity(
    long Id,
    PhysicalAddress MacAddress,
    IPAddress? IpAddress,
    int? ApiPort,
    byte? ApiVersion,
    DateTimeOffset LastHeartbeat);

/// <summary>
/// Defines logic for <see cref="StationEntity"/> that are not persisted directly in the database.
/// </summary>
public static class StationEntityExtensions
{
    #region Extension Methods
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
    /// <see langword="true"/> if the station is considered online, <see langword="false"/> otherwise.
    /// <see langword="null"/> if station status is ambiguous.
    /// </returns>
    public static bool? IsOnline(this StationEntity stationEntity) =>
        stationEntity switch
        {
            { IpAddress: null, ApiPort: null, ApiVersion: null } => false,
            { IpAddress: not null, ApiPort: not null, ApiVersion: not null } => true,
            _ => null
        };

/// <summary>
/// Determines the base URL for the API exposed by the station.
/// </summary>
/// <param name="stationEntity">
/// The station entity to determine the base URL for.
/// </param>
/// <returns>
/// Absolute base URL for the API exposed by the station.
/// <see langword="null"/> if the station is marked as offline.
/// </returns>
public static Uri? BaseApiUrl(this StationEntity stationEntity)
    {
        if (stationEntity.IpAddress is IPAddress ipAddress 
            && stationEntity.ApiPort is int apiPort
            && stationEntity.ApiVersion is byte apiVersion)
        {
            var builder = new UriBuilder(Uri.UriSchemeHttp, ipAddress.ToString(), apiPort, $"api/v{apiVersion}");
            return builder.Uri;
        }

        return null;
    }
    #endregion
}