using System.Net.NetworkInformation;

namespace Server.Data.Models.Dtos;

public sealed record StationDto(
    PhysicalAddress MacAddress,
    ElectricalSwitchDto[]? ElectricalSwitchDescriptors);
