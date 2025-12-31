namespace Server.Data.Models.Entities;

/// <summary>
/// Electrical switch controlled by a station.
/// </summary>
/// <param name="Identifier">
/// Unique electrical switch identifier assigned by database engine.
/// </param>
/// <param name="StationIdentifier">
/// Identifier of the station, which controlls the electrical switch.
/// </param>
/// <param name="LocalIdentifier">
/// Identifier used locally by the station to distinguish between multiple electrical switches.
/// </param>
/// <param name="Alias">
/// Text-based alias assigned to the electrical switch.
/// </param>
/// <param name="IsClosed">
/// State of the electrical switch - true if the cuirquit is closed and current is flowing, false otherwise.
/// </param>
public sealed record ElectricalSwitchEntity(
    long? Identifier,
    long StationIdentifier,
    byte LocalIdentifier,
    string? Alias,
    bool IsClosed);
