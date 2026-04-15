using System.Text;
using System.Text.Json;
using AdminService.DTOs;

namespace AdminService.Services;

/// <summary>
/// Implements outbound HTTP calls to UserService for KYC and profile administrative operations.
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<KycSubmissionDto>> GetPendingKycsAsync()
    {
        var response = await _httpClient.GetAsync("/api/user/admin/kyc/pending");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<KycSubmissionDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<KycSubmissionDto>();
    }

    public async Task<bool> ReviewKycAsync(int kycId, ReviewKycDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/api/user/admin/kyc/{kycId}/review", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<KycSubmissionDto?> GetKycByIdAsync(int kycId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/user/admin/kyc/{kycId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KycSubmissionDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            _logger.LogWarning("Failed to fetch KYC {KycId}", kycId);
            return null;
        }
    }

    public async Task<AdminUserProfileDto?> GetProfileAsync(int authUserId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/user/admin/users/{authUserId}/profile");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AdminUserProfileDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            _logger.LogWarning("Failed to fetch profile for AuthUserId {AuthUserId}", authUserId);
            return null;
        }
    }

    public async Task<KycSubmissionDto?> GetKycByAuthUserIdAsync(int authUserId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/user/admin/users/{authUserId}/kyc");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KycSubmissionDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            _logger.LogWarning("Failed to fetch KYC for AuthUserId {AuthUserId}", authUserId);
            return null;
        }
    }
}