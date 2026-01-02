using SmartHome.Server.Data.Models.Entities;
using System.Net;
using System.Net.NetworkInformation;

namespace Server.Data.Repositories;

/// <summary>
/// Defines interactions with repositories agreagting details about stations within the system.
/// </summary>
public interface IStationsRepository
{
    /// <summary>
    /// Creates a new station within the repository.
    /// </summary>
    /// <param name="macAddress">
    /// Unique MAC address of the station.
    /// </param>
    /// <param name="ipAddress">
    /// Unique IP address of the station.
    /// </param>
    /// <returns>
    /// Station model saved within the repository.
    /// </returns>
    Task<StationEntity> CreateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress);

    /// <summary>
    /// Retrivaes single station from the repository basing on provided criteria.
    /// </summary>
    /// <param name="macAddress">
    /// Filter by privided vaule of MAC address.
    /// </param>
    /// <param name="ignoreMacAddress">
    /// True, if filtering by MAC address shall be skipped, false otherwise.
    /// </param>
    /// <returns>
    /// Station model matching provided criteria.
    /// If no station stored within repository matches provided criteria, null reference is returned.
    /// </returns>
    Task<StationEntity?> GetSingleStationAsync(PhysicalAddress? macAddress = null, bool ignoreMacAddress = true);

    /// <summary>
    /// Retrivaes stations from the repository basing on provided criteria.
    /// </summary>
    /// <param name="macAddress">
    /// Filter by privided MAC address.
    /// If null reference, filtering by MAC address is skipped.
    /// </param>
    /// <param name="ignoreMacAddress">
    /// True, if filtering by MAC address shall be skipped, false otherwise.
    /// </param>
    /// <returns>
    /// Collection of station models matching provided criteria.
    /// </returns>
    Task<StationEntity[]> GetMultipleStationsAsync(PhysicalAddress? macAddress = null, bool ignoreMacAddress = true);

    /// <summary>
    /// Updates properties of station with specified ID.
    /// </summary>
    /// <param name="macAddress">
    /// Unique MAC address of the station, which shall be updated.
    /// </param>
    /// <param name="ipAddress">
    /// New IP address of the station.
    /// </param>
    /// <param name="ignoreIpAddress">
    /// True if updating station IP address shall be skipped, false otherwise.
    /// </param>
    /// <returns>
    /// Updated station model saved within the repository.
    /// If station with specified ID does not exist within the repository, null reference is returned.
    /// </returns>
    Task<StationEntity?> UpdateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress = null, bool ignoreIpAddress = true);
}
