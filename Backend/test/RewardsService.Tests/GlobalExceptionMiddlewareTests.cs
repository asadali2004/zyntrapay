using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RewardsService.Exceptions;
using RewardsService.Middleware;

namespace RewardsService.Tests;

[TestFixture]
public class GlobalExceptionMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonElement> ReadJsonResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<JsonElement>(body);
    }

    [Test]
    public async Task InvokeAsync_AppException_ReturnsStatusAndMessage()
    {
        var logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ConflictAppException("Reward already redeemed"),
            logger.Object);

        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));

        var json = await ReadJsonResponse(context);
        Assert.That(json.GetProperty("message").GetString(), Is.EqualTo("Reward already redeemed"));
        Assert.That(json.GetProperty("errorCode").GetString(), Is.EqualTo("CONFLICT"));
    }

    [Test]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        var logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new ArgumentException("Invalid payload"),
            logger.Object);

        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

        var json = await ReadJsonResponse(context);
        Assert.That(json.GetProperty("message").GetString(), Is.EqualTo("Invalid payload"));
        Assert.That(json.GetProperty("errorCode").GetString(), Is.EqualTo("VALIDATION_FAILED"));
    }

    [Test]
    public async Task InvokeAsync_UnhandledException_Returns500WithGenericMessage()
    {
        var logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new Exception("Boom"),
            logger.Object);

        var context = CreateHttpContext();

        await middleware.InvokeAsync(context);

        Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

        var json = await ReadJsonResponse(context);
        Assert.That(
            json.GetProperty("message").GetString(),
            Is.EqualTo("An unexpected error occurred."));
        Assert.That(json.GetProperty("errorCode").GetString(), Is.EqualTo("INTERNAL_SERVER_ERROR"));
    }
}
