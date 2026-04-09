using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UserService.Data;
using UserService.Models;
using ZyntraPay.IntegrationTests.Factories;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class UserIntegrationTests
{
    private UserWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new UserWebApplicationFactory();
        _client = _factory.CreateClient();

        var token = GenerateTestJwt(1, "john@example.com", "User");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static string GenerateTestJwt(int userId, string email, string role)
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKey12345678901234"));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Email, email),
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Role, role)
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    [Test]
    public async Task CreateProfile_ThenGetProfile_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/user/profile", new
        {
            fullName = "John Doe",
            dateOfBirth = "1995-06-15T00:00:00",
            address = "123 Main Street",
            city = "Mumbai",
            state = "Maharashtra",
            pinCode = "400001"
        });

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var getResponse = await _client.GetAsync("/api/user/profile");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await getResponse.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.That(json.GetProperty("fullName").GetString(), Is.EqualTo("John Doe"));
    }

    [Test]
    public async Task SubmitKyc_ThenGetStatus_ReturnsPending()
    {
        var submitResponse = await _client.PostAsJsonAsync("/api/user/kyc", new
        {
            documentType = "Aadhaar",
            documentNumber = "12345678"
        });

        Assert.That(submitResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var statusResponse = await _client.GetAsync("/api/user/kyc");
        Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await statusResponse.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.That(json.GetProperty("status").GetString(), Is.EqualTo("Pending"));
    }

    [Test]
    public async Task GetProfile_WhenMissing_Returns404()
    {
        var response = await _client.GetAsync("/api/user/profile");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
