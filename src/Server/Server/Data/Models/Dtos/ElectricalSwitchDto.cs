namespace Server.Data.Models.Dtos;

public sealed record ElectricalSwitchDto(
    byte LocalIdentifier,
    bool IsClosed);
