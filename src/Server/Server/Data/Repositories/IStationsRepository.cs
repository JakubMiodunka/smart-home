using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Repositories;

/// <summary>
/// Defines interactions with repositories aggregating details about stations within the system.
/// </summary>
public interface IStationsRepository
{
    /// <summary>
    /// Creates a new representation of station within the repository.
    /// </summary>
    /// <param name="macAddress">
    /// MAC address of the station.
    /// Shall be unique within the repository.
    /// </param>
    /// <param name="ipAddress">
    /// IP address of the station.
    /// </param>
    /// <returns>
    /// Station model saved within the repository.
    /// </returns>
    Task<StationEntity> CreateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress);

    /// <summary>
    /// Retrieves single station from the repository basing on provided criteria.
    /// </summary>
    /// <remarks>
    /// Currently filtering by ID is needed, but method is already prepared
    /// to support more filtering criteria if it will be needed in the future.
    /// </remarks>
    /// <param name="filterById">
    /// <see langword="true"/>, if filtering by ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="id">
    /// Value of ID by which stations shall be filtered.
    /// Ignored if value of <paramref name="filterById"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByIpAddress">
    /// <see langword="true"/>, if filtering by IP address shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="ipAddress">
    /// Value of IP address by which stations shall be filtered.
    /// Ignored if value of <paramref name="filterByIpAddress"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByMacAddress">
    /// <see langword="true"/>, if filtering by MAC address shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="macAddress">
    /// Value of MAC address by which stations shall be filtered.
    /// Ignored if value of <paramref name="filterByMacAddress"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The station that matches the provided criteria, or <see langword="null"/> reference if no match is found.
    /// </returns>
    Task<StationEntity?> GetSingleStationAsync(
        bool filterById = false, long? id = null,
        bool filterByIpAddress = false, IPAddress? ipAddress = null,
        bool filterByMacAddress = false, PhysicalAddress? macAddress = null);

    /// <summary>
    /// Updates properties of specified station.
    /// </summary>
    /// <remarks>
    /// Currently updating station IP address is needed, but method is already prepared
    /// to support multiple properties update if it will be needed in the future.
    /// </remarks>
    /// <param name="id">
    /// Specifies which station shall be updated.
    /// </param>
    /// <param name="updateIpAddress">
    /// <see langword="true"/> if IP address of specified station shall be updated, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="ipAddress">
    /// New IP address of the station.
    /// Ignored if value of <paramref name="updateIpAddress"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The updated station model saved within the repository.
    /// If the specified station does not exist, <see langword="null"/> reference is returned.
    /// </returns>
    Task<StationEntity?> UpdateStationAsync(long id, bool updateIpAddress = false, IPAddress? ipAddress = null);
}
