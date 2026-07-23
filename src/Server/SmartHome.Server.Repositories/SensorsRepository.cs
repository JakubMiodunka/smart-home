using Dapper;
using SmartHome.Server.Repositories.Abstractions;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Repositories.Enumerations;

namespace SmartHome.Server.Repositories;

/// <summary>
/// Repository providing access to data related to sensors.
/// </summary>
internal sealed class SensorsRepository : DatabaseClient, ISensorsRepository
{
    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SensorsRepository"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The connection string used to establish a connection to the database.
    /// </param>
    public SensorsRepository(string connectionString) : base(connectionString)
    {
        // Nothing to be done.
    }
    #endregion

    #region Interactions
    /// <inheritdoc cref="ISensorsRepository"/>
    public async Task<SensorEntity> CreateSensorAsync(
        long stationId,
        byte localId,
        MeasurementType measurementType)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@station_id", stationId);
        parameters.Add("@local_id", localId);
        parameters.Add("@measurement_type", measurementType);

        return await CreateEntityAsync<SensorEntity>("sensors_create", parameters);
    }

    /// <inheritdoc cref="ISensorsRepository"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required argument is a <see langword="null"/> reference.
    /// </exception>
    public async Task<SensorEntity?> GetSingleSensorAsync(
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

        return await GetSingleEntityAsync<SensorEntity>("sensors_get", parameters);
    }
    #endregion
}
