using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SmartHome.Server.Api.Controllers.Common;

/// <summary>
/// Base class for all controllers defined within the application.
/// </summary>
[ApiController]
[ServiceFilter(typeof(RequireRemoteIpAddressAttribute))]
public abstract class BaseController : ControllerBase
{
    #region Properties
    protected readonly IHttpContextAccessor _httpContextAccessor;

    public IPAddress RemoteIpAddress =>
        _httpContextAccessor.HttpContext?.Items[RequireRemoteIpAddressAttribute.RemoteIpAddressKey] is IPAddress remoteIpAddress ?
        remoteIpAddress
        : throw new InvalidOperationException("Remote IP address not available in HTTP context:");
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
}