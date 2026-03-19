using SmartHome.Server.Services.Processors;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Entities;

/// TODO: Concider adding timestamp of entity creation to avoid potential issues with processing outdated data.
/// <summary>
/// Entity representing the general details of a station functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <remarks>
/// In theory, type of <param name="ApiPort"/> param could be
/// set to <see langword="ushort"/> as its range fits the port range perfectly.
/// However, since SQL Server lacks an unsigned 16-bit type and <see cref="IPEndPoint.Port"/>
/// uses <see langword="int"/>, this type was used here to avoid unnecessary complexity and casting.
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
/// Set to <see langword="null"/> if station is offline and port is unknown.
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
    DateTimeOffset LastHeartbeat)
{
    /// <summary>
    /// Validates if the port is within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.
    /// </summary>
    /// <param name="port">
    /// The port number to validate.
    /// </param>
    /// <returns>
    /// The validated port number.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the port is out of valid range.
    /// </exception>
    private static int? ValidatePort(int? port)
    {
        if (port is int value)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, IPEndPoint.MinPort, nameof(port));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, IPEndPoint.MaxPort, nameof(port));
        }
        return port;
    }

    public int? ApiPort { get; init; } = ValidatePort(ApiPort);
}

/// <summary>
/// Defines logic for <see cref="StationEntity"/> that are not persisted directly in the database.
/// </summary>
public static class StationEntityExtensions
{
    #region Constants
    private const string StationApiPrefix = "api/v1";   // TODO: Save to DB also station API version.
    #endregion

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
        stationEntity.IpAddress is not null && stationEntity.ApiPort is not null;

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
        if (stationEntity.IpAddress is IPAddress ipAddress && stationEntity.ApiPort is int apiPort)
        {
            var builder = new UriBuilder(Uri.UriSchemeHttp, ipAddress.ToString(), apiPort, StationApiPrefix);
            return builder.Uri;
        }

        return null;
    }
    #endregion
}