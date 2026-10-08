using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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
    /// <summary>
    /// The key used to store the remote IP address in <see cref="HttpContext.Items"/>.
    /// </summary>
    public const string RemoteIpAddressKey = "RemoteIpAddress";
    #endregion

    #region Properties
    private readonly ILogger<RequireRemoteIpAddressAttribute>? _logger;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireRemoteIpAddressAttribute"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is intended primarily for unit testing.
    /// </remarks>
    /// <param name="logger">
    /// The logger, which shall be used by created filter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    internal RequireRemoteIpAddressAttribute(ILogger<RequireRemoteIpAddressAttribute> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireRemoteIpAddressAttribute"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is intended for normal application use.
    /// The logger is resolved from <see cref="HttpContext.RequestServices"/> during action execution.
    ///
    /// Filters applied directly as attributes cannot receive constructor dependencies through
    /// dependency injection. Using constructor injection would require registering the attribute 
    /// as a service and applying it ex. via <c>[ServiceFilter]</c>, which adds unnecessary complexity.
    /// </remarks>
    public RequireRemoteIpAddressAttribute()
    {
        // Nothing to be done.
    }

    /// <summary>
    /// Gets the logger, which shall be used while processing the current request.
    /// </summary>
    /// <param name="actionContext">
    /// The <see cref="ActionExecutingContext"/> for the currently processed request.
    /// </param>
    /// <returns>
    /// The logger to use while processing the current request.
    /// </returns>
    private ILogger<RequireRemoteIpAddressAttribute> GetLogger(ActionExecutingContext actionContext) =>
        _logger ?? actionContext.HttpContext.RequestServices.GetRequiredService<ILogger<RequireRemoteIpAddressAttribute>>();

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
        ILogger<RequireRemoteIpAddressAttribute> logger = GetLogger(context);

        IPAddress? remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;
        
        var endPointUrl = new Uri(
            $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}",
            UriKind.Relative);

        if (remoteIpAddress is null)
        {
            logger.LogWarning(
                "New request rejected: " +
                "Message=[{Message}], EndPoint=[{EndPoint}] HttpMethod=[{HttpMethod}]",
                "Failed to determine client IP address.",
                endPointUrl,
                context.HttpContext.Request.Method);

            context.Result = new BadRequestResult();
            return;
        }

        context.HttpContext!.Items.Add(RemoteIpAddressKey, remoteIpAddress);

        logger.LogDebug(
            "New request accepted:" +
            "EndPoint=[{EndPoint}], HttpMethod=[{HttpMethod}], RemoteIpAddress=[{RemoteIpAddress}]",
            endPointUrl,
            context.HttpContext.Request.Method,
            remoteIpAddress);

        base.OnActionExecuting(context);
    }
}
