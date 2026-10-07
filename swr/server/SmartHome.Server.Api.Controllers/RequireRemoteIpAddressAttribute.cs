using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SmartHome.Server.Api.Controllers;

/// <summary>
/// Action filter applied to API controllers to ensure that callers of its endpoints 
/// can be identified by their IP address.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class RequireRemoteIpAddressAttribute : ActionFilterAttribute
{
    #region Constants
    public const string RemoteIpAddressKey = "RemoteIpAddress";
    #endregion

    #region Properties
    private readonly ILogger<RequireRemoteIpAddressAttribute> _logger;
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="RequireRemoteIpAddressAttribute"/>.
    /// </summary>
    /// <param name="logger">
    /// Logger which shall be used by created attribute instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public RequireRemoteIpAddressAttribute(ILogger<RequireRemoteIpAddressAttribute> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
    }

    /// <summary>
    /// Extracts the remote IP address before executing the action in the controller.
    /// </summary>
    /// <remarks>
    /// The extracted IP address is stored in <see cref="HttpContext.Items"/>
    /// under the key <see cref="RemoteIpAddressKey"/>.
    /// If the remote IP address cannot be determined, the request is rejected
    /// with an HTTP 400 Bad Request response.
    /// </remarks>
    /// <param name="context">
    /// Context of action executed in controller.
    /// </param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IPAddress? remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;

        if (remoteIpAddress is null)
        {
            _logger.LogWarning(
                "New request rejected: Message=[{Message}], EndPoint=[{EndPoint}]",
                "Failed to determine client IP address.",
                context.ActionDescriptor.DisplayName);

            context.Result = new BadRequestResult();
            return;
        }

        context.HttpContext!.Items.Add(RemoteIpAddressKey, remoteIpAddress);

        _logger.LogDebug(
                "New request accepted: EndPoint=[{EndPoint}], RemoteIpAddress=[{RemoteIpAddress}]",
                context.ActionDescriptor.DisplayName,
                remoteIpAddress);

        base.OnActionExecuting(context);
    }
}
