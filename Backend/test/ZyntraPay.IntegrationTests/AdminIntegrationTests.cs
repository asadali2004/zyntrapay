using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AdminService.DTOs;
using NUnit.Framework;
using ZyntraPay.IntegrationTests.Factories;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class AdminIntegrationTests
{
    private AdminWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new AdminWebApplicationFactory();
        _client = _factory.CreateClient();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestJwt(99, "admin@zyntrapay.com", "Admin"));
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private string GenerateTestJwt(int userId, string email, string role)
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
    public async Task GetPendingKycs_AuthenticatedAdmin_Returns200WithData()
    {
        _factory.Data.PendingKycs = new List<KycSubmissionDto>
        {
            new() { Id = 1, AuthUserId = 10, Status = "Pending", DocumentType = "PAN", DocumentNumber = "ABCDE1234F" }
        };

        var response = await _client.GetAsync("/api/admin/kyc/pending");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(json.GetArrayLength(), Is.EqualTo(1));
    }

    [Test]
    public async Task ReviewKyc_ValidRequest_Returns200()
    {
        _factory.Data.PendingKycs = new List<KycSubmissionDto>
        {
            new() { Id = 1, AuthUserId = 10, Status = "Pending", DocumentType = "PAN", DocumentNumber = "ABCDE1234F" }
        };

        _factory.Data.Users = new List<UserSummaryDto>
        {
            new() { Id = 10, Email = "john@example.com", IsActive = true }
        };

        var response = await _client.PutAsJsonAsync("/api/admin/kyc/1/review", new
        {
            status = "Approved"
        });

        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("successfully").IgnoreCase);
    }

    [Test]
    public async Task GetDashboard_AuthenticatedAdmin_Returns200WithCounts()
    {
        _factory.Data.Users = new List<UserSummaryDto>
        {
            new() { Id = 1, IsActive = true, Email = "u1@example.com" },
            new() { Id = 2, IsActive = false, Email = "u2@example.com" }
        };

        _factory.Data.PendingKycs = new List<KycSubmissionDto>
        {
            new() { Id = 1, Status = "Pending" },
            new() { Id = 2, Status = "Pending" }
        };

        var response = await _client.GetAsync("/api/admin/dashboard");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.GetProperty("totalUsers").GetInt32(), Is.EqualTo(2));
        Assert.That(json.GetProperty("activeUsers").GetInt32(), Is.EqualTo(1));
        Assert.That(json.GetProperty("pendingKyc").GetInt32(), Is.EqualTo(2));
    }
}
