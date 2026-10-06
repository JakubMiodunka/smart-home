using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace SmartHome.Server.Api.Controllers;

internal sealed class RequireRemoteIpAddressAttribute : ActionFilterAttribute
{
    public const string RemoteIpAddressKey = "RemoteIpAddress";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IPAddress? remoteIpAddress = context.HttpContext?.Connection.RemoteIpAddress;

        if (remoteIpAddress is null)
        {
            context.Result = new BadRequestResult();
            return;
        }

        context.HttpContext!.Items.Add(RemoteIpAddressKey, remoteIpAddress);

        base.OnActionExecuting(context);
    }
}
