using System.ComponentModel.DataAnnotations;

namespace SmartHome.Server.Api.Controllers.Clients.Requests;

/// <summary>
/// Data transfer object (DTO) representing a request created by server client
/// to update properties of electrical switch on server site.
/// </summary>
/// <param name="ExpectedSwitchState">
/// Desired state of the switch.
/// <see langword="true"/> if the circuit shall be closed 
/// and current shall flow; <see langword="false"/> otherwise.
/// </param>
public sealed record SwitchUpdateRequest([Required] bool ExpectedSwitchState);
