namespace Server.Data.Models.Descriptors.Api;

public sealed record ElectricalSwitchApiDescriptor(
    byte LocalIdentifier,
    bool IsClosed);
