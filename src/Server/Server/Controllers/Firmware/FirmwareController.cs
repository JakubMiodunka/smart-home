using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SmartHome.Server.Controllers.Firmware;

/// <summary>
/// Base class for controllers that handle requests from station firmware.
/// </summary>
[ApiController]
public abstract class FirmwareController : ControllerBase
{
    #region Properties
    protected readonly IHttpContextAccessor _httpContextAccessor;
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes basic functionalities of the <see cref="FirmwareController"/>.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    protected FirmwareController(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Attempts to retrieve the remote IP address of the client from the current HTTP context.
    /// </summary>
    /// <param name="ipAddress">
    /// Contains the remote IP address of the client if attempt was successful,
    /// <see langword="null"/> otherwise.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the IP address was successfully retrieved,
    /// <see langword="false"/> otherwise.
    /// </returns>
    protected bool TryGetRemoteIpAddress(out IPAddress? ipAddress)
    {
        ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
        return ipAddress is not null;
    }
    #endregion
}