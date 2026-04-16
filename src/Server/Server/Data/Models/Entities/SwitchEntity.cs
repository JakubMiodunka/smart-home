using SmartHome.Server.Services.Processors;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;

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
    long StationId, // TODO: Rename to ParentStationId.
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
    /// <param name="switchEntity">T
    /// Switch entity to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the switch is considered online.
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsOnline(this SwitchEntity switchEntity) =>
        switchEntity.ActualState is not null;

    /// <summary>
    /// Determines the URL of API endpoint which controls the switch.
    /// </summary>
    /// <param name="switchEntity">
    /// Entity of the switch which API endpoint URL shall be determined.
    /// </param>
    /// <param name="parentStation">
    /// Entity of the station that controls the switch.
    /// </param>
    /// <returns>
    /// Absolute URL of API endpoint which controls the switch.
    /// <see langword="null"/> if endpoint is considered as unreachable.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    /// /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="parentStation"/> is not the parent station of <paramref name="switchEntity"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when generation of switch URL is not supported for API version of <paramref name="parentStation"/>.
    /// </exception>
    public static Uri? SwitchUrl(this SwitchEntity switchEntity, StationEntity parentStation)
    {
        ArgumentNullException.ThrowIfNull(parentStation, nameof(parentStation));
        ArgumentOutOfRangeException.ThrowIfNotEqual(switchEntity.StationId, parentStation.Id, nameof(parentStation));

        if (parentStation.BaseApiUrl() is not Uri stationApiUrl) return null;

        Uri switchApiEndpoint = parentStation.ApiVersion switch
        {
            1 => new Uri($"switches/{switchEntity.LocalId}", UriKind.Relative),
            _ => throw new NotSupportedException($"Station API version not supported: ApiVersion=[{parentStation.ApiVersion}]")
        };

        return new Uri(stationApiUrl, switchApiEndpoint);
    }
}