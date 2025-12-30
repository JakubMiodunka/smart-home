using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models;

public sealed record StationDescriptor(
    long? Identifier,
    PhysicalAddress MacAddress,
    IPAddress? IpAddress,
    string? Alias);
