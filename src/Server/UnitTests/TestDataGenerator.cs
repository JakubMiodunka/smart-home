using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace UnitTests;

internal static class TestDataGenerator
{
    public static Mock<IHttpContextAccessor> CreateHttpContextAccessorFake(IPAddress remoteIpAddress)
    {
        ArgumentNullException.ThrowIfNull(remoteIpAddress, nameof(remoteIpAddress));

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = remoteIpAddress;

        var httpContextAccessorFake = new Mock<IHttpContextAccessor>();
        httpContextAccessorFake.Setup(fake => fake.HttpContext).Returns(httpContext);

        return httpContextAccessorFake;
    }
}
