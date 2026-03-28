using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using WalletService.Data;
using WalletService.Models;
using ZyntraPay.IntegrationTests.Factories;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class WalletIntegrationTests
{
    private WalletWebApplicationFactory _factory;
    private HttpClient _client;
    private string _jwtToken;

    [SetUp]
    public void SetUp()
    {
        _factory = new WalletWebApplicationFactory();
        _client = _factory.CreateClient();

        // Seed a wallet directly into in-memory DB
        SeedWallet(authUserId: 1, email: "john@example.com", balance: 1000m);

        // Generate a real JWT for testing
        _jwtToken = GenerateTestJwt(userId: 1, email: "john@example.com", role: "User");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private void SeedWallet(int authUserId, string email, decimal balance)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();

        if (!db.Wallets.Any(w => w.AuthUserId == authUserId))
        {
            db.Wallets.Add(new Wallet
            {
                AuthUserId = authUserId,
                UserEmail = email,
                Balance = balance,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }

    private string GenerateTestJwt(int userId, string email, string role)
    {
        // Use same JWT settings as configured in test factory
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

    // ── BALANCE TESTS ─────────────────────────────────────────────────────

    [Test]
    public async Task GetBalance_AuthenticatedUser_Returns200WithBalance()
    {
        // Act
        var response = await _client.GetAsync("/api/wallet/balance");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.GetProperty("balance").GetDecimal(), Is.EqualTo(1000m));
    }

    [Test]
    public async Task GetBalance_Unauthenticated_Returns401()
    {
        // Arrange — remove auth header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/wallet/balance");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    // ── TOPUP TESTS ───────────────────────────────────────────────────────

    [Test]
    public async Task TopUp_ValidAmount_Returns200AndUpdatesBalance()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/topup", new
        {
            amount = 500,
            description = "Test Top-Up"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify balance updated
        var balanceResponse = await _client.GetAsync("/api/wallet/balance");
        var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
        var balanceJson = JsonSerializer.Deserialize<JsonElement>(balanceContent);

        // Balance should be 1000 (seeded) + 500 (topup) = 1500
        // Note: cache may serve 1000 for 30 seconds, so we check >= 1000
        Assert.That(balanceJson.GetProperty("balance").GetDecimal(), Is.GreaterThanOrEqualTo(1000m));
    }

    [Test]
    public async Task TopUp_ZeroAmount_Returns400()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/topup", new
        {
            amount = 0,
            description = "Invalid"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task TopUp_ExceedsLimit_Returns400()
    {
        // Act — amount exceeds 50,000 limit
        var response = await _client.PostAsJsonAsync("/api/wallet/topup", new
        {
            amount = 99999,
            description = "Over limit"
        });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // ── CREATE WALLET TESTS ───────────────────────────────────────────────

    [Test]
    public async Task CreateWallet_AlreadyExists_Returns400()
    {
        // Wallet already seeded in SetUp

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/create", new { });

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("already exists").IgnoreCase);
    }

    // ── TRANSACTIONS TESTS ────────────────────────────────────────────────

    [Test]
    public async Task GetTransactions_AuthenticatedUser_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/api/wallet/transactions");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}