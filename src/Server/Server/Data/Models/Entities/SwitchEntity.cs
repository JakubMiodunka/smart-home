using SmartHome.Server.Services.Processors;

namespace SmartHome.Server.Data.Models.Entities;

/// <summary>
/// Entity representing the details of an electrical switch functioning within the system.
/// Used for data exchange between the server and the database.
/// </summary>
/// <param name="Id">
/// The unique global identifier for the electrical switch.
/// </param>
/// <param name="StationId">
/// The unique identifier of the station that controls this switch.
/// </param>
/// <param name="LocalId">
/// The identifier of the switch, unique only at the station level.
/// </param>
/// <param name="ExpectedState">
/// Expected state of the electrical switch.
/// <see langword="true"/> if the circuit shall be closed 
/// and current shall flow; <see langword="false"/> otherwise.
/// </param>
/// <param name="ActualState">
/// Actual state of the electrical switch. <see langword="true"/> if the circuit is closed 
/// and current is flowing; <see langword="false"/> otherwise.
/// <see langword="null"/> indicates the state is unknown.
/// </param>
public sealed record SwitchEntity(
    long Id,
    long StationId,
    byte LocalId,
    bool ExpectedState,
    bool? ActualState);

/// <summary>
/// Defines logic for <see cref="SwitchEntity"/> that are not persisted directly in the database.
/// </summary>
public static class SwitchEntityExtensions
{
    /// <summary>
    /// Determines whether the switch is considered online.
    /// </summary>
    /// <remarks>
    /// A switch is defined as online only if its actual state is known. 
    /// The <see cref="HeartbeatMonitoringServiceProcessor"> is responsible 
    /// for nullifying the actual switch state if a heartbeat timeout occurs.
    /// </remarks>
    /// <param name="stationEntity">T
    /// Switch entity to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the switch is considered online.
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsOnline(this SwitchEntity switchEntity) =>
        switchEntity.ActualState is not null;
}