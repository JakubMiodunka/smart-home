using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Data.Repositories;

/// <summary>
/// Defines interactions with repositories aggregating details about electrical switches within the system.
/// </summary>
public interface IElectricalSwitchesRepository
{
    /// <summary>
    /// Creates a new representation of electrical switch within the repository.
    /// </summary>
    /// <param name="stationId">
    /// The unique identifier of the station that controls the switch.
    /// </param>
    /// <param name="localId">
    /// The identifier of the switch, unique only at the station level.
    /// </param>
    /// <param name="isClosed">
    /// The state of the electrical switch. <see langword="true"/> if the circuit is closed 
    /// and current is flowing; <see langword="false"/> otherwise. 
    /// A <see langword="null"/> value indicates the state is unknown.
    /// </param>
    /// <returns>
    /// Electrical switch model saved within the repository.
    /// </returns>
    Task<ElectricalSwitchEntity> CreateElectricalSwitchAsync(long stationId, byte localId, bool? isClosed);

    /// <summary>
    /// Retrieves single electrical switch from the repository basing on provided criteria.
    /// </summary>
    /// <remarks>
    /// Currently filtering by ID is needed, but method is already prepared
    /// to support more filtering criteria if it will be needed in the future.
    /// </remarks>
    /// <param name="filterById">
    /// <see langword="true"/>, if filtering by electrical switch ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="id">
    /// Value of ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterById"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The electrical switch that matches the provided criteria, or <see langword="null"/> reference if no match is found.
    /// </returns>
    Task<ElectricalSwitchEntity?> GetSingleElectricalSwitchAsync(bool filterById = false, long? id = null);

    /// <summary>
    /// Retrieves electrical switches from the repository basing on provided criteria.
    /// </summary>
    /// <remarks>
    /// Currently filtering by station ID is needed, but method is already prepared
    /// to support more filtering criteria if it will be needed in the future.
    /// </remarks>
    /// <param name="filterByStationId">
    /// <see langword="true"/>, if filtering by ID of station that controls the switch
    /// shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="stationId">
    /// Value of station ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterByStationId"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Collection of electrical switches that matches the provided criteria.
    /// </returns>
    Task<ElectricalSwitchEntity[]> GetMultipleElectricalSwitchesAsync(bool filterByStationId = false, long? stationId = null);

    /// <summary>
    /// Updates properties of specified electrical switch.
    /// </summary>
    /// <remarks>
    /// Currently updating electrical switch state is needed, but method is already prepared
    /// to support multiple properties update if it will be needed in the future.
    /// </remarks>
    /// <param name="id">
    /// Specifies ID of electrical switch which shall be updated.
    /// </param>
    /// <param name="updateState">
    /// <see langword="true"/> if state of specified electrical switch shall be updated, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="isClosed">
    /// New state of the electrical switch.
    /// Ignored if value of <paramref name="updateState"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The updated electrical switch model saved within the repository.
    /// If the specified switch does not exist, <see langword="null"/> reference is returned.
    /// </returns>
    Task<ElectricalSwitchEntity?> UpdateElectricalSwitchAsync(long id, bool updateState = false, bool? isClosed = null);
}
