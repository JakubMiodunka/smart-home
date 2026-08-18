using Dapper;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Repositories;

/// <summary>
/// Repository providing access to data related to switches.
/// </summary>
internal sealed class SwitchesRepository : DatabaseClient, ISwitchesRepository
{
    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchesRepository"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The connection string used to establish a connection to the database.
    /// </param>
    public SwitchesRepository(string connectionString) : base(connectionString)
    {
        // Nothing to be done.
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="ISwitchesRepository"/>
    public async Task<SwitchEntity> CreateSwitchAsync(long stationId, byte localId, bool expectedState, bool? actualState)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@station_id", stationId);
        parameters.Add("@local_id", localId);
        parameters.Add("@expected_state", expectedState);
        parameters.Add("@actual_state", actualState);

        return await CreateEntityAsync<SwitchEntity>("switches_create", parameters);
    }

    /// <inheritdoc cref="ISwitchesRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<SwitchEntity?> GetSingleSwitchAsync(
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

        return await GetSingleEntityAsync<SwitchEntity>("switches_get", parameters);
    }

    /// <inheritdoc cref="ISwitchesRepository"/>
    public async Task<SwitchEntity[]> GetMultipleSwitchesAsync(bool filterByStationId = false, long? stationId = null)
    {
        var parameters = new DynamicParameters();

        if (filterByStationId)
        {
            ArgumentNullException.ThrowIfNull(stationId, nameof(stationId));

            parameters.Add("@filter_by_station_id", filterByStationId);
            parameters.Add("@station_id", stationId);
        }

        return await GetMultipleEntitiesAsync<SwitchEntity>("switches_get", parameters);
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

        parameters.Add("@update_actual_state", updateActualState);
        parameters.Add("@actual_state", updateActualState ? actualState : null);

        return await GetSingleEntityAsync<SwitchEntity>("switches_update", parameters);
    }
    #endregion
}
