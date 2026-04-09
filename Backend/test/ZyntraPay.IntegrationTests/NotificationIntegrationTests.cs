using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NotificationService.Data;
using NotificationService.Models;
using ZyntraPay.IntegrationTests.Factories;

namespace ZyntraPay.IntegrationTests;

[TestFixture]
public class NotificationIntegrationTests
{
    private NotificationWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private string _jwtToken = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new NotificationWebApplicationFactory();
        _client = _factory.CreateClient();

        SeedNotification(authUserId: 1, title: "Welcome", message: "Your account is ready.");

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

    private void SeedNotification(int authUserId, string title, string message)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        db.Notifications.Add(new Notification
        {
            AuthUserId = authUserId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
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
    public async Task GetAll_AuthenticatedUser_Returns200WithList()
    {
        var response = await _client.GetAsync("/api/notification");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(json.GetArrayLength(), Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task MarkAsRead_ValidId_Returns200()
    {
        var notificationId = GetFirstNotificationId();
        var response = await _client.PutAsync($"/api/notification/{notificationId}/read", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_Unauthenticated_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/notification");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private int GetFirstNotificationId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        return db.Notifications.Select(n => n.Id).First();
    }
}
