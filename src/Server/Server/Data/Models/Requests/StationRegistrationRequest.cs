using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace SmartHome.Server.Data.Models.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request to register a station within the system.
/// </summary>
/// <remarks>
/// Used for data exchange between the server and station firmware.
/// </remarks>
/// <param name="MacAddress">
/// The MAC address of the station to be registered.
/// </param>
public sealed record StationRegistrationRequest([Required] PhysicalAddress MacAddress);
