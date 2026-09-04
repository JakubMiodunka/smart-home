using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SmartHome.Server.Repositories.Abstractions;

/// <summary>
/// Database client handling interactions with the database.
/// </summary>
/// <remarks>
/// This class is dedicated solely to executing SQL code.
/// If there is a need to introduce additional logic exceeding pure data access, 
/// it must be implemented in a separate repository class dedicated to a specific entity type.
/// </remarks>
internal abstract class DatabaseClient
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
    protected DatabaseClient(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        _connectionString = connectionString;
    }
    #endregion

    #region Utilities
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
    protected async Task<T> CreateEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
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
    protected async Task<T?> GetSingleEntityAsync<T>(string procedureName, DynamicParameters parameters) where T : class
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
    protected async Task<T[]> GetMultipleEntitiesAsync<T>(string procedureName, DynamicParameters parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName, nameof(procedureName));
        ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));

        using var connection = new SqlConnection(_connectionString);
        IEnumerable<T> entities = await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        return entities.ToArray();
    }
    #endregion
}
