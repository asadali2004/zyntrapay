using System.Net.Http.Json;
using System.Text.Json;

namespace ZyntraPay.IntegrationTests.Helpers;

public static class TestAuthHelper
{
    public static async Task<string> GetTokenAsync(
        HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        return json.GetProperty("token").GetString() ?? string.Empty;
    }

    public static async Task SeedVerifiedUserAsync(
        HttpClient client, string email, string phone, string password)
    {
        // Directly call send-otp then verify-otp then register
        await client.PostAsJsonAsync("/api/auth/send-otp", new { email });

        // For testing — we need to get the OTP from cache
        // We'll use a workaround: call verify with a test endpoint
        // Instead, use the test-only bypass below
    }
}