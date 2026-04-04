using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SmartHome.UnitTests;

internal static class IActionResultTestingUtilities
{
    #region Generic assertions
    private static void AssertStatusCodeActionResult<TResult>(
        IActionResult actionResultUnderTest,
        int expectedStatusCode)
        where TResult : IStatusCodeActionResult
    {
        Assert.That(actionResultUnderTest, Is.Not.Null);
        Assert.That(actionResultUnderTest, Is.InstanceOf<TResult>());

        var specificActionResult = (IStatusCodeActionResult)actionResultUnderTest;

        Assert.That(specificActionResult.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    private static void AssertObjectResult<TResult, TValue>(
        IActionResult actionResultUnderTest,
        int expectedStatusCode,
        TValue? expectedValue)
        where TResult : ObjectResult
        where TValue : class
    {
        AssertStatusCodeActionResult<TResult>(actionResultUnderTest, expectedStatusCode);

        var objectResult = (ObjectResult)actionResultUnderTest;

        Assert.That(objectResult.Value, Is.InstanceOf<TValue>());
        Assert.That(objectResult.Value, Is.EqualTo(expectedValue));
    }
    #endregion

    #region Specific assertions
    public static void AssertOkObjectResult<TValue>(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status200OK,
        TValue? expectedValue = null)
        where TValue : class =>
        AssertObjectResult<OkObjectResult, TValue>(actionResultUnderTest, expectedStatusCode, expectedValue);

    public static void AssertNoContentResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status204NoContent) => 
        AssertStatusCodeActionResult<NoContentResult>(actionResultUnderTest, expectedStatusCode);

    public static void AssertCreatedAtActionResult<TValue>(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status201Created,
        string? expectedControllerName = null,
        string? expectedActionName = null,
        TValue? expectedValue = null)
        where TValue : class
    {
        AssertObjectResult<OkObjectResult, TValue>(actionResultUnderTest, expectedStatusCode, expectedValue);

        var createdAtActionResult = (CreatedAtActionResult)actionResultUnderTest;

        Assert.Multiple(() =>
        {
            Assert.That(createdAtActionResult.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(createdAtActionResult.ControllerName, Is.EqualTo(expectedControllerName));
            Assert.That(createdAtActionResult.ActionName, Is.EqualTo(expectedActionName));
        });
    }

    public static void AssertBadRequestResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status400BadRequest) =>
        AssertStatusCodeActionResult<BadRequestResult>(actionResultUnderTest, expectedStatusCode);

    public static void AssertBadRequestObjectResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status400BadRequest) =>
        AssertStatusCodeActionResult<BadRequestObjectResult>(actionResultUnderTest, expectedStatusCode);

    public static void AssertNotFoundObjectResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status404NotFound) =>
        AssertStatusCodeActionResult<NotFoundObjectResult>(actionResultUnderTest, expectedStatusCode);

    public static void AssertNotFoundResult(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status404NotFound) =>
        AssertStatusCodeActionResult<NotFoundResult>(actionResultUnderTest, expectedStatusCode);

    public static void AssertInternalServerError(
        this IActionResult actionResultUnderTest,
        int expectedStatusCode = StatusCodes.Status500InternalServerError) =>
        AssertStatusCodeActionResult<StatusCodeResult>(actionResultUnderTest, expectedStatusCode);
    #endregion
}
