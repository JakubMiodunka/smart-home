using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to register a station within the system.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="StationMacAddress">
/// The MAC address of the station to be registered.
/// </param>
/// <param name="StationApiPort">
/// The network port on which the station's control service is listening.
/// </param>
public sealed record StationRegistrationStationRequest(
    [Required]
    PhysicalAddress StationMacAddress,

    [Required]
    [Range(IPEndPoint.MinPort, IPEndPoint.MaxPort)]
    int StationApiPort);
