using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RewardsService.Data;
using RewardsService.Models;
using ZyntraPay.IntegrationTests.Factories;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class RewardsIntegrationTests
{
    private RewardsWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new RewardsWebApplicationFactory();
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
    public async Task GetCatalog_Returns200()
    {
        SeedCatalog();

        var response = await _client.GetAsync("/api/rewards/catalog");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task GetSummary_WhenMissing_Returns404()
    {
        var response = await _client.GetAsync("/api/rewards/summary");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetSummary_WhenAccountExists_Returns200()
    {
        SeedAccount();

        var response = await _client.GetAsync("/api/rewards/summary");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.That(json.GetProperty("totalPoints").GetInt32(), Is.EqualTo(120));
    }

    private void SeedCatalog()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RewardsDbContext>();

        if (!db.RewardCatalogs.Any())
        {
            db.RewardCatalogs.Add(new RewardCatalog
            {
                Title = "Coffee Voucher",
                Description = "Test item",
                PointsCost = 100,
                Stock = 10,
                IsActive = true
            });
            db.SaveChanges();
        }
    }

    private void SeedAccount()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RewardsDbContext>();

        if (!db.RewardAccounts.Any(a => a.AuthUserId == 1))
        {
            db.RewardAccounts.Add(new RewardAccount
            {
                AuthUserId = 1,
                TotalPoints = 120,
                Tier = "Silver",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}
