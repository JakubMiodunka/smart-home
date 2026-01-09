using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Dtos;

/// <summary>
/// Data transfer object (DTO) representing the general details of a station functioning within the system.
/// Used for data exchange between the server and the API clients.
/// </summary>
/// <param name="MacAddress">
/// The unique physical MAC address of the station, used as a primary identifier in the network.
/// </param>
public sealed record StationDto(
    [Required] PhysicalAddress MacAddress);
