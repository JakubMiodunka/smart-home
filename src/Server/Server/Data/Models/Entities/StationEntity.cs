using System.Net;
using System.Net.NetworkInformation;

namespace Server.Data.Models.Entities;

public sealed record StationEntity(
    long? Identifier,
    PhysicalAddress MacAddress,
    IPAddress? IpAddress,
    string? Alias);
