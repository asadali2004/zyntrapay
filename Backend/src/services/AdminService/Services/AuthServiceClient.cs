using System.Text.Json;
using AdminService.DTOs;

namespace AdminService.Services;

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(HttpClient httpClient, ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<UserSummaryDto>> GetAllUsersAsync()
    {
        var response = await _httpClient.GetAsync("/api/auth/admin/users");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<UserSummaryDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<UserSummaryDto>();
    }

    public async Task<bool> ToggleUserStatusAsync(int userId)
    {
        var response = await _httpClient.PutAsync($"/api/auth/admin/users/{userId}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetUserEmailAsync(int authUserId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/auth/users/{authUserId}/email");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            _logger.LogWarning("Failed to fetch email for AuthUserId {AuthUserId}", authUserId);
            return null;
        }
    }
}