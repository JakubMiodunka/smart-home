using System.Net;
using System.Net.NetworkInformation;

namespace Server.Data.Models.Descriptors.Database;

public sealed record StationDatabaseDescriptor(
    long? Identifier,
    PhysicalAddress MacAddress,
    IPAddress? IpAddress,
    string? Alias);
