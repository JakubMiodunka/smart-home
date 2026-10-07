using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Controllers;
using SmartHome.Server.Tests.Utilities;
using System.Net;

namespace SmartHome.Server.Tests.Api.Controllers;

[Category("UnitTest")]
[TestOf(typeof(RequireRemoteIpAddressAttribute))]
[Author("Jakub Miodunka")]
internal sealed class RequireRemoteIpAddressAttributeTests
{
    #region Test utilities
    private static ActionExecutingContext CreateActionExecutingContext(IPAddress? remoteIpAddress)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = remoteIpAddress;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(new RouteValueDictionary()),
            new ActionDescriptor()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new Mock<Controller>().Object
        );
    }
    #endregion

    #region Test cases
    [Test]
    public void CapturesRemoteIpAddress()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress validIpAddress = randomizer.NextIpAddress();
        ActionExecutingContext context = CreateActionExecutingContext(validIpAddress);

        var loggerMock = new FakeLogger<RequireRemoteIpAddressAttribute>();

        var attributeUnderTest = new RequireRemoteIpAddressAttribute(loggerMock);
        attributeUnderTest.OnActionExecuting(context);

        Assert.That(context.HttpContext.Items[RequireRemoteIpAddressAttribute.RemoteIpAddressKey], Is.TypeOf<IPAddress>());
        Assert.That(context.HttpContext.Items[RequireRemoteIpAddressAttribute.RemoteIpAddressKey], Is.EqualTo(validIpAddress));
        Assert.That(context.Result, Is.Null);

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Debug));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Debug < record.Level));
    }

    [Test]
    public void ReturnsBadRequestIfRemoteIpAddressNotAvailable()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        ActionExecutingContext context = CreateActionExecutingContext(remoteIpAddress: null);

        var loggerMock = new FakeLogger<RequireRemoteIpAddressAttribute>();

        var attributeUnderTest = new RequireRemoteIpAddressAttribute(loggerMock);
        attributeUnderTest.OnActionExecuting(context);

        Assert.That(context.HttpContext.Items.ContainsKey(RequireRemoteIpAddressAttribute.RemoteIpAddressKey), Is.False);
        Assert.That(context.Result, Is.Not.Null);
        context.Result.AssertBadRequestResult();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => record.Level == LogLevel.Warning));
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Warning < record.Level));
    }
    #endregion
}
