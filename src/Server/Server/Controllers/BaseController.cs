using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Server.Controllers;

/// <summary>
/// Base class for all controllers defined within the application.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    #region Properties
    protected readonly IHttpContextAccessor _httpContextAccessor;
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes basic functionalities of the <see cref="BaseController"/>.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the <see cref="HttpContext"/> of the current request.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one required reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    protected BaseController(IHttpContextAccessor httpContextAccessor) : base()
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
    }
    #endregion

    #region Utilities
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