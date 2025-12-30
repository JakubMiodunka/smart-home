using Dapper;
using Microsoft.Data.SqlClient;
using Server.Data.Models.Descriptors.Database;
using System.Data;

namespace SmartHome.Server.Data;

public sealed class DataAccessor
{
    private readonly string _connectionString;

    public DataAccessor(string connectionString) =>
        _connectionString = connectionString;

    private async Task<T> CreateEntity<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<StationDatabaseDescriptor> CreateStation(StationDatabaseDescriptor stationDescriptor)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", stationDescriptor.MacAddress);
        parameters.Add("@ip_address", stationDescriptor.IpAddress);
        parameters.Add("@alias", stationDescriptor.Alias, size: 100);

        return await CreateEntity<StationDatabaseDescriptor>("create_station", parameters);
    }

    public async Task<ElectricalSwitchApiDescriptor> CreatePowerSwitch(ElectricalSwitchApiDescriptor electricalSwitchDescriptor)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@station_identifier", electricalSwitchDescriptor.StationIdentifier);
        parameters.Add("@local_identifier", electricalSwitchDescriptor.LocalIdentifier);
        parameters.Add("@alias", electricalSwitchDescriptor.Alias, size: 100);
        parameters.Add("@is_closed", electricalSwitchDescriptor.IsClosed);
        
        return await CreateEntity<ElectricalSwitchApiDescriptor>("create_electrical_switch", parameters);
    }
}
