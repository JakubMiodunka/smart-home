using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Data.Repositories;

/// <summary>
/// Defines interactions with repositories aggregating details about electrical switches within the system.
/// </summary>
public interface ISwitchesRepository
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
    /// <param name="expectedState">
    /// Expected state of the switch.
    /// <see langword="true"/> if the circuit shall be closed 
    /// and current shall flow; <see langword="false"/> otherwise.
    /// </param>
    /// <param name="actualState">
    /// Actual state of the switch. <see langword="true"/> if the circuit is closed 
    /// and current is flowing; <see langword="false"/> otherwise.
    /// <see langword="null"/> indicates the state is unknown.
    /// </param>
    /// <returns>
    /// Switch entity saved within the repository.
    /// </returns>
    Task<SwitchEntity> CreateSwitchAsync(long stationId, byte localId, bool expectedState, bool? actualState);

    /// <summary>
    /// Retrieves single electrical switch from the repository basing on provided criteria.
    /// </summary>
    /// <param name="filterById">
    /// <see langword="true"/>, if filtering by switch ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="id">
    /// Value of ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterById"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByStationId">
    /// <see langword="true"/>, if filtering by station ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="stationId">
    /// Value of station ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterByStationId"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="filterByLocalId">
    /// <see langword="true"/>, if filtering by local ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="localId">
    /// Value of local ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterByLocalId"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Switch entity that matches the provided criteria, or <see langword="null"/> reference if no match is found.
    /// </returns>
    Task<SwitchEntity?> GetSingleSwitchAsync(
        bool filterById = false,
        long? id = null,
        bool filterByStationId = false,
        long? stationId = null,
        bool filterByLocalId = false,
        byte? localId = null);

    /// <summary>
    /// Retrieves collection of electrical switches from the repository basing on provided criteria.
    /// </summary>
    /// <param name="filterByStationId">
    /// <see langword="true"/>, if filtering by station ID shall be applied, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="stationId">
    /// Value of station ID by which switches shall be filtered.
    /// Ignored if value of <paramref name="filterByStationId"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Collection of switch entities that matches the provided criteria.
    /// </returns>
    Task<SwitchEntity[]> GetMultipleSwitchesAsync(bool filterByStationId = false, long? stationId = null);

    /// <summary>
    /// Updates properties of specified electrical switch.
    /// </summary>
    /// <param name="id">
    /// Specifies ID of switch which shall be updated.
    /// </param>
    /// <param name="updateExpectedState">
    /// <see langword="true"/> if expected state of specified switch shall be updated, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="expectedState">
    /// New value of expected state for specified switch.
    /// Ignored if value of <paramref name="updateExpectedState"/> is set to <see langword="false"/>.
    /// </param>
    /// <param name="updateActualState">
    /// <see langword="true"/> if expected state of specified switch shall be updated, <see langword="false"/> otherwise.
    /// </param>
    /// <param name="actualState">
    /// New value of expected state for specified switch.
    /// Ignored if value of <paramref name="updateActualState"/> is set to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// The updated switch entity saved within the repository.
    /// If the specified switch does not exist, <see langword="null"/> reference is returned.
    /// </returns>
    Task<SwitchEntity?> UpdateSwitchAsync(
        long id,
        bool updateExpectedState = false,
        bool? expectedState = null,
        bool updateActualState = false,
        bool? actualState = null);
}
