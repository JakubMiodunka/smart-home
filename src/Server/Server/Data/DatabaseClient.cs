using Dapper;
using Microsoft.Data.SqlClient;
using Server.Data.Models.Entities;
using System.Data;

namespace SmartHome.Server.Data;

public sealed class DatabaseClient
{
    private readonly string _connectionString;

    public DatabaseClient(string connectionString) =>
        _connectionString = connectionString;

    private async Task<T> CreateEntity<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<StationEntity> CreateStation(StationEntity stationDescriptor)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", stationDescriptor.MacAddress);
        parameters.Add("@ip_address", stationDescriptor.IpAddress);
        parameters.Add("@alias", stationDescriptor.Alias, size: 100);

        return await CreateEntity<StationEntity>("SP_station_create", parameters);
    }

    public async Task<bool> IsStationExists(string macAddress)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<bool>("SP_station_exists", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<ElectricalSwitchEntity> CreatePowerSwitch(ElectricalSwitchEntity electricalSwitchDescriptor)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@station_identifier", electricalSwitchDescriptor.StationIdentifier);
        parameters.Add("@local_identifier", electricalSwitchDescriptor.LocalIdentifier);
        parameters.Add("@alias", electricalSwitchDescriptor.Alias, size: 100);
        parameters.Add("@is_closed", electricalSwitchDescriptor.IsClosed);
        
        return await CreateEntity<ElectricalSwitchEntity>("SP_electrical_switch_create", parameters);
    }
}
