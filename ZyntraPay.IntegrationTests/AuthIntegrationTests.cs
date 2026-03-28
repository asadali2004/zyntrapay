using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ZyntraPay.IntegrationTests.Factories;
using ZyntraPay.IntegrationTests.Helpers;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class AuthIntegrationTests
{
    private AuthWebApplicationFactory _factory;
    private HttpClient _client;

    [SetUp]
    public void SetUp()
    {
        _factory = new AuthWebApplicationFactory();
        _client = _factory.CreateClient();

        // Seed a test user directly into in-memory DB
        TestDataSeeder.SeedUser(
            _factory.Services,
            id: 1,
            email: "john@example.com",
            phone: "9876543210",
            password: "Test@123"
        );
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ── LOGIN TESTS ──────────────────────────────────────────────────────

    [Test]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "john@example.com",
            password = "Test@123"
        });

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.TryGetProperty("token", out _), Is.True);
        Assert.That(json.GetProperty("token").GetString(), Is.Not.Empty);
    }

    [Test]
    public async Task Login_WrongPassword_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "john@example.com",
            password = "WrongPassword"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_NonExistentEmail_Returns401()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@example.com",
            password = "Test@123"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_MissingEmail_Returns400()
    {
        // Act — send empty email (validation should catch it)
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "",
            password = "Test@123"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Login_ReturnsCorrectRole()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "john@example.com",
            password = "Test@123"
        });

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.That(json.GetProperty("role").GetString(), Is.EqualTo("User"));
        Assert.That(json.GetProperty("email").GetString(), Is.EqualTo("john@example.com"));
    }

    // ── SEND OTP TESTS ────────────────────────────────────────────────────

    [Test]
    public async Task SendOtp_ValidEmail_Returns200()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/send-otp", new
        {
            email = "newuser@example.com"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task SendOtp_InvalidEmailFormat_Returns400()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/send-otp", new
        {
            email = "not-an-email"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // ── REGISTER WITHOUT OTP TESTS ────────────────────────────────────────

    [Test]
    public async Task Register_WithoutOtpVerification_Returns400()
    {
        // Act — try to register without verifying OTP first
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "fresh@example.com",
            phoneNumber = "9111111111",
            password = "Test@123"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("not verified").IgnoreCase);
    }
}