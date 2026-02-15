using Dapper;
using Microsoft.Data.SqlClient;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Database;

/// <summary>
/// Database client handling interactions with the database.
/// </summary>
/// <remarks>
/// This interface is primarily created to support repository classes whose capabilities 
/// may exceed pure SQL execution. Such classes shall interact with the database through this interface.
/// </remarks>
public interface IDatabaseClient : IStationsRepository, ISwitchesRepository;

/// <inheritdoc cref="IDatabaseClient" path="/summary"/>
/// <remarks>
/// This class is dedicated solely to executing SQL code.
/// If there is a need to introduce additional logic exceeding pure data access, 
/// it must be implemented in a separate repository class dedicated to a specific entity type.
/// </remarks>
public sealed class DatabaseClient : IDatabaseClient
{
    #region Properties
    private readonly string _connectionString;
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes a new instance of database client.
    /// </summary>
    /// <param name="connectionString">
    /// The connection string used to establish a connection to the database.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    public DatabaseClient(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        _connectionString = connectionString;
    }
    #endregion

    #region Anxulary methods
    /// <summary>
    /// Creates new entity within the database using specified SQL procedure.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the entity to be created.
    /// </typeparam>
    /// <param name="procedureName">
    /// Name of the SQL procedure which shall be used to create the new entity.
    /// </param>
    /// <param name="parameters">
    /// Collection of parameters required to execute the specified procedure.
    /// </param>
    /// <returns>
    /// Representation of entity saved in database.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    private async Task<T> CreateEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName, nameof(procedureName));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Retrieves single entity from the database using specified SQL procedure.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the entity to be retrieved.
    /// </typeparam>
    /// <param name="procedureName">
    /// Name of the SQL procedure which shall be used to retrieve entity.
    /// </param>
    /// <param name="parameters">
    /// Collection of parameters required to execute the specified procedure.
    /// </param>
    /// <returns>
    /// The entity retrieved from the database, or <see langword="null"/> if the database does not return any entities.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    private async Task<T?> GetSingleEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName, nameof(procedureName));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Retrieves multiple entities from the database using specified SQL procedure.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the entities to be retrieved.
    /// </typeparam>
    /// <param name="procedureName">
    /// Name of the SQL procedure which shall be used to retrieve entities.
    /// </param>
    /// <param name="parameters">
    /// Collection of parameters required to execute the specified procedure.
    /// </param>
    /// <returns>
    /// Collection of entities retrieved from the database.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one of provided arguments is invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    private async Task<T[]> GetMultipleEntitiesAsync<T>(string procedureName, DynamicParameters parameters) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName, nameof(procedureName));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

        using var connection = new SqlConnection(_connectionString);
        IEnumerable<T> entities = await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        return entities.ToArray();
    }
    #endregion

    #region Stations
    /// <inheritdoc cref="IStationsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity> CreateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress, DateTime lastHeartbeat)
    {
        ArgumentNullException.ThrowIfNull(macAddress, nameof(macAddress));

        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ip_address", ipAddress);
        parameters.Add("@last_heartbeat", lastHeartbeat);

        return await CreateEntityAsync<StationEntity>("SP_stations_create", parameters);
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

        return await GetSingleEntityAsync<StationEntity>("SP_stations_get", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    public async Task<StationEntity[]> GetMultipleStationsAsync()
    {
        var parameters = new DynamicParameters();

        return await GetMultipleEntitiesAsync<StationEntity>("SP_stations_get", parameters);
    }

    /// <inheritdoc cref="IStationsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity?> UpdateStationAsync(
        long id,
        bool updateIpAddress = false, IPAddress? ipAddress = null,
        bool updateLastHeartbeat = false, DateTime? lastHeartbeat = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);

        if (updateIpAddress)
        {
            parameters.Add("@update_ip_address", updateIpAddress);
            parameters.Add("@ip_address", ipAddress);
        }

        if (updateLastHeartbeat)
        {
            ArgumentNullException.ThrowIfNull(lastHeartbeat, nameof(lastHeartbeat));

            parameters.Add("@update_last_heartbeat", updateLastHeartbeat);
            parameters.Add("@last_heartbeat", lastHeartbeat);
        }

        return await GetSingleEntityAsync<StationEntity>("SP_stations_update", parameters);
    }
    #endregion

    #region Switches
    /// <inheritdoc cref="ISwitchesRepository"/>
    public async Task<SwitchEntity> CreateSwitchAsync(long stationId, byte localId, bool expectedState, bool? actualState)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@station_id", stationId);
        parameters.Add("@local_id", localId);
        parameters.Add("@expected_state", expectedState);
        parameters.Add("@actual_state", actualState);

        return await CreateEntityAsync<SwitchEntity>("SP_switches_create", parameters);
    }

    /// <inheritdoc cref="ISwitchesRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async  Task<SwitchEntity?> GetSingleSwitchAsync(
        bool filterById = false, long? id = null,
        bool filterByStationId = false, long? stationId = null,
        bool filterByLocalId = false, byte? localId = null)
    {
        var parameters = new DynamicParameters();

        if (filterById)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));

            parameters.Add("@filter_by_id", filterById);
            parameters.Add("@id", id);
        }

        if (filterByStationId)
        {
            ArgumentNullException.ThrowIfNull(stationId, nameof(stationId));

            parameters.Add("@filter_by_station_id", filterByStationId);
            parameters.Add("@station_id", stationId);
        }

        if (filterByLocalId)
        {
            ArgumentNullException.ThrowIfNull(localId, nameof(localId));

            parameters.Add("@filter_by_local_id", filterByLocalId);
            parameters.Add("@local_id", localId);
        }

        return await GetSingleEntityAsync<SwitchEntity>("SP_switches_get", parameters);
    }

    /// <inheritdoc cref="ISwitchesRepository"/>
    public async Task<SwitchEntity[]> GetMultipleSwitchesAsync(
        bool filterByStationId = false,
        long? stationId = null,
        bool filterByActualState = true,
        bool? actualState = null)
    {
        var parameters = new DynamicParameters();

        if (filterByStationId)
        {
            ArgumentNullException.ThrowIfNull(stationId, nameof(stationId));

            parameters.Add("@filter_by_station_id", filterByStationId);
            parameters.Add("@station_id", stationId);
        }

        if (filterByActualState)
        {
            parameters.Add("@filter_by_actual_state", filterByStationId);
            parameters.Add("@actual_state", stationId);
        }

        return await GetMultipleEntitiesAsync<SwitchEntity>("SP_switches_get", parameters);
    }

    /// <inheritdoc cref="ISwitchesRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<SwitchEntity?> UpdateSwitchAsync(
        long id,
        bool updateExpectedState = false, bool? expectedState = null,
        bool updateActualState = false, bool? actualState = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);

        if (updateExpectedState)
        {
            ArgumentNullException.ThrowIfNull(expectedState, nameof(expectedState));

            parameters.Add("@update_expected_state", updateExpectedState);
            parameters.Add("@expected_state", expectedState);
        }

        if (updateActualState)
        {
            parameters.Add("@update_actual_state", updateActualState);
            parameters.Add("@actual_state", actualState);
        }

        return await GetSingleEntityAsync<SwitchEntity>("SP_switches_update", parameters);
    }
    #endregion
}
