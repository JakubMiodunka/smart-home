using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SmartHome.UnitTests;

internal static class TestDataGenerator
{
    public static Mock<IHttpContextAccessor> CreateHttpContextAccessorFake(IPAddress? remoteIpAddress = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = remoteIpAddress;

        var httpContextAccessorFake = new Mock<IHttpContextAccessor>();
        httpContextAccessorFake.Setup(fake => fake.HttpContext).Returns(httpContext);

        return httpContextAccessorFake;
    }

    public static Utf8JsonReader CreateJsonReader(string readerContent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readerContent, nameof(readerContent));

        string jsonString = JsonSerializer.Serialize(readerContent);
        byte[] encodedJsonString = Encoding.UTF8.GetBytes(jsonString);
        var jsonReader = new Utf8JsonReader(encodedJsonString);
        jsonReader.Read();

        return jsonReader;
    }
}
