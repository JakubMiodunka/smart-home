using Dapper;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Repositories;

/// <summary>
/// Repository providing access to data related to stations.
/// </summary>
internal sealed class StationsRepository : DatabaseClient, IStationsRepository
{
    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="StationsRepository"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The connection string used to establish a connection to the database.
    /// </param>
    public StationsRepository(string connectionString) : base(connectionString)
    {
        // Nothing to be done.
    }
    #endregion

    #region Internactions
    /// <inheritdoc cref="IStationsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity> CreateStationAsync(
        PhysicalAddress macAddress,
        IPAddress? ipAddress,
        int? apiPort,
        byte? apiVersion,
        DateTimeOffset lastHeartbeat)
    {
        ArgumentNullException.ThrowIfNull(macAddress, nameof(macAddress));

        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ip_address", ipAddress);
        parameters.Add("@api_port", apiPort);
        parameters.Add("@api_version", apiVersion);
        parameters.Add("@last_heartbeat", lastHeartbeat.ToUniversalTime());

        return await CreateEntityAsync<StationEntity>("stations_create", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity?> GetSingleStationAsync(
        bool filterById = false, long? id = null,
        bool filterByIpAddress = false, IPAddress? ipAddress = null,
        bool filterByMacAddress = false, PhysicalAddress? macAddress = null)
    {
        var parameters = new DynamicParameters();

        if (filterById)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            parameters.Add("@filter_by_id", filterById);
            parameters.Add("@id", id);
        }

        if (filterByIpAddress)
        {
            ArgumentNullException.ThrowIfNull(ipAddress, nameof(ipAddress));

            parameters.Add("@filter_by_ip_address", filterByIpAddress);
            parameters.Add("@ip_address", ipAddress);
        }

        if (filterByMacAddress)
        {
            ArgumentNullException.ThrowIfNull(macAddress, nameof(macAddress));

            parameters.Add("@filter_by_mac_address", filterByMacAddress);
            parameters.Add("@mac_address", macAddress);
        }

        return await GetSingleEntityAsync<StationEntity>("stations_get", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    public async Task<StationEntity[]> GetMultipleStationsAsync()
    {
        var parameters = new DynamicParameters();

        return await GetMultipleEntitiesAsync<StationEntity>("stations_get", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity?> UpdateStationAsync(
        long id,
        bool updateIpAddress = false, IPAddress? ipAddress = null,
        bool updateApiPort = false, int? apiPort = null,
        bool updateApiVersion = false, byte? apiVersion = null,
        bool updateLastHeartbeat = false, DateTimeOffset? lastHeartbeat = null)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", id);

        parameters.Add("@update_ip_address", updateIpAddress);
        parameters.Add("@ip_address", updateIpAddress ? ipAddress : null);

        parameters.Add("@update_api_port", updateApiPort);
        parameters.Add("@api_port", updateApiPort ? apiPort : null);

        parameters.Add("@update_api_version", updateApiVersion);
        parameters.Add("@api_version", updateApiVersion ? apiVersion : null);

        if (updateLastHeartbeat)
        {
            ArgumentNullException.ThrowIfNull(lastHeartbeat, nameof(lastHeartbeat));

            parameters.Add("@update_last_heartbeat", updateLastHeartbeat);
            parameters.Add("@last_heartbeat", lastHeartbeat?.ToUniversalTime());
        }

        return await GetSingleEntityAsync<StationEntity>("stations_update", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    public async Task<long[]> MarkOfflineStations(DateTimeOffset minHeartbeatTimestamp)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@min_heartbeat_timestamp", minHeartbeatTimestamp);

        return await GetMultipleEntitiesAsync<long>("stations_mark_offline", parameters);
    }
    #endregion
}
