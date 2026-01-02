using Dapper;
using Microsoft.Data.SqlClient;
using SmartHome.Server.Data.Models.Entities;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;

namespace Server.Data.Database;

public sealed class DatabaseClient : IDatabaseClient
{
    private readonly string _connectionString;

    public DatabaseClient(string connectionString) =>
        _connectionString = connectionString;

    #region Anxulary methods
    private async Task<T> CreateEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    private async Task<T?> GetSingleEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    private async Task<T[]> GetMultipleEntitiesAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        IEnumerable<T> entities = await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        return entities.ToArray();
    }
    #endregion

    #region Stations
    public async Task<StationEntity> CreateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ip_address", ipAddress);

        return await CreateEntityAsync<StationEntity>("SP_stations_create", parameters);
    }

    public async Task<StationEntity?> GetSingleStationAsync(PhysicalAddress? macAddress = null, bool ignoreMacAddress = true)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ignore_mac_address", ignoreMacAddress);

        return await GetSingleEntityAsync<StationEntity>("SP_stations_get", parameters);
    }

    public async Task<StationEntity[]> GetMultipleStationsAsync(PhysicalAddress? macAddress = null, bool ignoreMacAddress = true)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ignore_mac_address", ignoreMacAddress);

        return await GetMultipleEntitiesAsync<StationEntity>("SP_stations_get", parameters);
    }

    public async Task<StationEntity?> UpdateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress = null, bool ignoreIpAddress = true)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ip_address", ipAddress);
        parameters.Add("@ignore_ip_address", ignoreIpAddress);

        return await GetSingleEntityAsync<StationEntity>("SP_stations_update", parameters);
    }
    #endregion
}
