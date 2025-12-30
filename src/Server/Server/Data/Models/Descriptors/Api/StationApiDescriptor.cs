using System.Net;
using System.Net.NetworkInformation;

namespace Server.Data.Models.Descriptors.Api;

public sealed record StationApiDescriptor(
    PhysicalAddress MacAddress);
