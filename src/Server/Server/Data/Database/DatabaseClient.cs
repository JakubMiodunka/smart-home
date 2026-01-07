using Dapper;
using Microsoft.Data.SqlClient;
using SmartHome.Server.Data.Models.Entities;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Database;

/// <summary>
/// Database client handling interactions with the database.
/// </summary>
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

    #region Data access - anxulary methods
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

    #region Data access - stations
    /// <summary>
    /// Creates a new representation of station within the database.
    /// </summary>
    /// <param name="macAddress">
    /// MAC address of the station.
    /// Shall be unique within the system.
    /// </param>
    /// <param name="ipAddress">
    /// IP address of the station.
    /// </param>
    /// <returns>
    /// Representation of station entity saved in database.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<StationEntity> CreateStationAsync(PhysicalAddress macAddress, IPAddress? ipAddress)
    {
        ArgumentNullException.ThrowIfNull(macAddress, nameof(macAddress));

        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@ip_address", ipAddress);

        return await CreateEntityAsync<StationEntity>("SP_stations_create", parameters);
    }

    /// <summary>
    /// Retrieves single station from the database basing on provided criteria.
    /// </summary>
    /// <param name="filterById">
    /// <see langword="true"/>, if filtering by ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="macAddress">
    /// Value of ID by which stations shall be filtered.
    /// Ignored if value of <paramref name="filterById"/> is set to <see langword="false"/>.
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
    public async Task<StationEntity?> GetSingleStationAsync(
        bool filterById = false, long? id = null,
        bool filterByMacAddress = false, PhysicalAddress? macAddress = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@filter_by_id", filterById);
        parameters.Add("@id", id);
        parameters.Add("@filter_by_mac_address", filterByMacAddress);
        parameters.Add("@mac_address", macAddress);

        return await GetSingleEntityAsync<StationEntity>("SP_stations_get", parameters);
    }

    /// <summary>
    /// Retrieves stations from the database basing on provided criteria.
    /// </summary>
    /// <param name="filterByMacAddress">
    /// <see langword="true"/>, if filtering by MAC address shall be applied, <see langword="false"/> otherwise.
    /// Ignored if value of <paramref name="filterByMacAddress"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="macAddress">
    /// Value of MAC address by which stations shall be filtered.
    /// </param>
    /// <returns>
    /// Collection of stations that match the provided criteria.
    /// </returns>
    public async Task<StationEntity[]> GetMultipleStationsAsync(bool filterByMacAddress = false, PhysicalAddress? macAddress = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@filter_by_mac_address", filterByMacAddress);
        parameters.Add("@mac_address", macAddress);

        return await GetMultipleEntitiesAsync<StationEntity>("SP_stations_get", parameters);
    }

    /// <summary>
    /// Updates properties of specified station.
    /// </summary>
    /// <param name="macAddress">
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
    /// The updated station model saved within the database.
    /// If the specified station does not exist, <see langword="null"/> reference is returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a null reference.
    /// </exception>
    public async Task<StationEntity?> UpdateStationAsync(PhysicalAddress macAddress, bool updateIpAddress = false, IPAddress? ipAddress = null)
    {
        ArgumentNullException.ThrowIfNull(macAddress, nameof(macAddress));

        var parameters = new DynamicParameters();
        parameters.Add("@mac_address", macAddress);
        parameters.Add("@update_ip_address", updateIpAddress);
        parameters.Add("@ip_address", ipAddress);

        return await GetSingleEntityAsync<StationEntity>("SP_stations_update", parameters);
    }
    #endregion
}
