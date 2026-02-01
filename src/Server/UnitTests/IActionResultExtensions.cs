using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartHome.UnitTests;

internal static class IActionResultExtensions
{
    public static void AssertOkObjectResult<T>(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status200OK,
        T? expectedValue = null) where T : class, IEquatable<T>
    {
        Assert.That(actionResultUnderTest, Is.Not.Null);
        Assert.That(actionResultUnderTest, Is.InstanceOf<OkObjectResult>());

        var specificActionResult = (OkObjectResult)actionResultUnderTest;

        Assert.That(specificActionResult.StatusCode, Is.EqualTo(expectedStatusCode));

        Assert.That(specificActionResult.Value, Is.InstanceOf<T>());
        Assert.That(specificActionResult.Value, Is.EqualTo(expectedValue));
    }

    public static void AssertNoContentResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status204NoContent)
    {
        Assert.That(actionResultUnderTest, Is.Not.Null);
        Assert.That(actionResultUnderTest, Is.InstanceOf<NoContentResult>());

        var specificActionResult = (NoContentResult)actionResultUnderTest;

        Assert.That(specificActionResult.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    public static void AssertCreatedAtActionResult<T>(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status201Created,
        string? expectedControllerName = null,
        string? expectedActionName = null,
        T? expectedValue = null) where T : class, IEquatable<T>
    {
        Assert.That(actionResultUnderTest, Is.Not.Null);
        Assert.That(actionResultUnderTest, Is.InstanceOf<CreatedAtActionResult>());

        var specificActionResult = (CreatedAtActionResult)actionResultUnderTest;

        Assert.Multiple(() =>
        {
            Assert.That(specificActionResult.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(specificActionResult.ControllerName, Is.EqualTo(expectedControllerName));
            Assert.That(specificActionResult.ActionName, Is.EqualTo(expectedActionName));
        });

        Assert.That(specificActionResult.Value, Is.InstanceOf<T>());
        Assert.That(specificActionResult.Value, Is.EqualTo(expectedValue));
    }

    public static void AssertBadRequestObjectResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status400BadRequest)
    {
        Assert.That(actionResultUnderTest, Is.Not.Null);
        Assert.That(actionResultUnderTest, Is.InstanceOf<BadRequestObjectResult>());

        var specificActionResult = (BadRequestObjectResult)actionResultUnderTest;

        Assert.That(specificActionResult.StatusCode, Is.EqualTo(expectedStatusCode));
    }
}
