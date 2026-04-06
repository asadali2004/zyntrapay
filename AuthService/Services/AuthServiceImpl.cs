using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Google.Apis.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = new();
    public int ExpiryMinutes { get; set; }
}

public class AuthServiceImpl : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly JwtSettings _jwt;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceImpl> _logger;
    private readonly IMemoryCache _cache;
    private readonly IRabbitMqPublisher _publisher;

    public AuthServiceImpl(
        IAuthRepository repo,
        IOptions<JwtSettings> jwt,
        IConfiguration configuration,
        ILogger<AuthServiceImpl> logger,
        IMemoryCache cache,
        IRabbitMqPublisher publisher)
    {
        _repo = repo;
        _jwt = jwt.Value;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        _publisher = publisher;
    }

    public Task<(bool Success, string Message)> SendOtpAsync(SendOtpRequestDto dto)
    {
        _logger.LogInformation("OTP requested for email: {Email}", dto.Email);

        var normalizedEmail = dto.Email.ToLower();
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = $"otp_{normalizedEmail}";

        _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(10));

        _publisher.Publish(new OtpRequestedEvent
        {
            Email = normalizedEmail,
            Otp = otp,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("OTP sent for email: {Email}", normalizedEmail);
        return Task.FromResult((true, "OTP sent to your email. Valid for 10 minutes."));
    }

    public Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequestDto dto)
    {
        var normalizedEmail = dto.Email.ToLower();
        var cacheKey = $"otp_{normalizedEmail}";

        if (!_cache.TryGetValue(cacheKey, out string? storedOtp))
            return Task.FromResult((false, "OTP expired or not found. Please request a new OTP."));

        if (storedOtp != dto.Otp)
            return Task.FromResult((false, "Invalid OTP. Please try again."));

        _cache.Remove(cacheKey);
        _cache.Set($"verified_{normalizedEmail}", true, TimeSpan.FromMinutes(15));

        _logger.LogInformation("OTP verified for email: {Email}", normalizedEmail);
        return Task.FromResult((true, "Email verified successfully. You can now complete registration."));
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto)
    {
        _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        var normalizedEmail = dto.Email.ToLower();

        // Check OTP was verified
        if (!_cache.TryGetValue($"verified_{normalizedEmail}", out bool verified) || !verified)
            return (false, "Email not verified. Please verify OTP before registering.");

        if (await _repo.EmailExistsAsync(normalizedEmail))
            return (false, "Email already registered.");

        if (await _repo.PhoneExistsAsync(dto.PhoneNumber))
            return (false, "Phone number already registered.");

        var user = new User
        {
            Email = normalizedEmail,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddUserAsync(user);
        await _repo.SaveChangesAsync();

        // Remove verified flag after registration
        _cache.Remove($"verified_{normalizedEmail}");

        // Publish welcome email event
        _publisher.Publish(new WelcomeEmailRequestedEvent
        {
            Email = normalizedEmail,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("User registered successfully: {Email}", normalizedEmail);
        return (true, "Registration successful.");
    }

    public async Task<(bool Success, string Message)> RegisterAdminAsync(AdminRegisterRequestDto dto)
    {
        // Validate secret key
        var expectedKey = _configuration["AdminSettings:SecretKey"];
        if (dto.AdminSecretKey != expectedKey)
            return (false, "Invalid admin secret key.");

        if (await _repo.EmailExistsAsync(dto.Email))
            return (false, "Email already registered.");

        if (await _repo.PhoneExistsAsync(dto.PhoneNumber))
            return (false, "Phone number already registered.");

        var user = new User
        {
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Admin",    
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddUserAsync(user);
        await _repo.SaveChangesAsync();

        return (true, "Admin registered successfully.");
    }
    public async Task<(bool Success, AuthResponseDto? Data, string Message)> LoginAsync(LoginRequestDto dto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email); 

        var normalizedEmail = dto.Email.ToLower();
        var user = await _repo.GetByEmailAsync(normalizedEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email); 
            return (false, null, "Invalid email or password.");
        }

        if (!user.IsActive)
            return (false, null, "Account is deactivated.");

        _logger.LogInformation("Login successful for email: {Email}", dto.Email); 
        return (true, BuildAuthResponse(user), "Login successful.");
    }

    public async Task<(bool Success, AuthResponseDto? Data, string Message)> GoogleLoginAsync(
        GoogleLoginRequestDto dto)
    {
        _logger.LogInformation("Google login attempt");

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["GoogleAuth:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            var normalizedEmail = payload.Email.ToLower();

            var existingUser = await _repo.GetByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                if (!existingUser.IsActive)
                    return (false, null, "Account is deactivated.");

                _logger.LogInformation("Google login successful for: {Email}", normalizedEmail);
                return (true, BuildAuthResponse(existingUser), "Login successful.");
            }

            var temporaryPhone = await GenerateUniqueTemporaryPhoneAsync();
            var newUser = new User
            {
                Email = normalizedEmail,
                PhoneNumber = temporaryPhone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddUserAsync(newUser);
            await _repo.SaveChangesAsync();

            _publisher.Publish(new WelcomeEmailRequestedEvent
            {
                Email = newUser.Email,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("New user auto-registered via Google: {Email}", normalizedEmail);

            var response = BuildAuthResponse(newUser);
            response.PhoneUpdateRequired = true;
            return (true, response, "Registration via Google successful.");
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("Invalid Google token: {Message}", ex.Message);
            return (false, null, "Invalid Google token. Please try again.");
        }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
        var normalizedEmail = dto.Email.ToLower();
        var user = await _repo.GetByEmailAsync(normalizedEmail);

        if (user != null)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();
            _cache.Set($"reset_otp_{normalizedEmail}", otp, TimeSpan.FromMinutes(10));

            _publisher.Publish(new OtpRequestedEvent
            {
                Email = normalizedEmail,
                Otp = otp,
                Timestamp = DateTime.UtcNow
            });
        }

        return (true, "If this email is registered, a reset OTP has been sent.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var normalizedEmail = dto.Email.ToLower();
        var cacheKey = $"reset_otp_{normalizedEmail}";

        if (!_cache.TryGetValue(cacheKey, out string? storedOtp))
            return (false, "OTP expired or not found. Please request a new reset OTP.");

        if (storedOtp != dto.Otp)
            return (false, "Invalid OTP.");

        var user = await _repo.GetByEmailAsync(normalizedEmail);
        if (user == null)
            return (false, "User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _repo.SaveChangesAsync();

        _cache.Remove(cacheKey);
        RevokeUserRefreshToken(user.Id);

        return (true, "Password reset successful.");
    }

    public async Task<(bool Success, AuthResponseDto? Data, string Message)> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        if (!_cache.TryGetValue($"refresh_token_{dto.RefreshToken}", out int userId))
            return (false, null, "Invalid or expired refresh token.");

        var user = await _repo.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            return (false, null, "User not found or inactive.");

        _cache.Remove($"refresh_token_{dto.RefreshToken}");
        return (true, BuildAuthResponse(user), "Token refreshed successfully.");
    }

    public async Task<(bool Success, string Message)> UpdatePhoneAsync(int userId, UpdatePhoneDto dto)
    {
        if (await _repo.PhoneExistsAsync(dto.PhoneNumber))
            return (false, "Phone number already in use.");

        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        user.PhoneNumber = dto.PhoneNumber;
        await _repo.SaveChangesAsync();

        return (true, "Phone number updated successfully.");
    }

    private async Task<string> GenerateUniqueTemporaryPhoneAsync()
    {
        for (var i = 0; i < 20; i++)
        {
            var candidate = $"1{Random.Shared.Next(0, 1_000_000_000):D9}";
            if (!await _repo.PhoneExistsAsync(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Failed to generate a unique temporary phone number.");
    }

    private bool IsTemporaryPhone(string phoneNumber)
        => !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.StartsWith("1");

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        foreach (var audience in GetTokenAudiences())
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private IEnumerable<string> GetTokenAudiences()
    {
        if (_jwt.Audiences is { Count: > 0 })
            return _jwt.Audiences.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct();

        return string.IsNullOrWhiteSpace(_jwt.Audience)
            ? []
            : [_jwt.Audience];
    }

    private AuthResponseDto BuildAuthResponse(User user)
        => new()
        {
            Token = GenerateToken(user),
            RefreshToken = IssueRefreshToken(user.Id),
            Email = user.Email,
            Role = user.Role,
            PhoneUpdateRequired = IsTemporaryPhone(user.PhoneNumber)
        };

    private string IssueRefreshToken(int userId)
    {
        RevokeUserRefreshToken(userId);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiry = TimeSpan.FromDays(7);

        _cache.Set($"refresh_token_{refreshToken}", userId, expiry);
        _cache.Set($"user_refresh_{userId}", refreshToken, expiry);

        return refreshToken;
    }

    private void RevokeUserRefreshToken(int userId)
    {
        if (_cache.TryGetValue($"user_refresh_{userId}", out string? existingToken))
        {
            _cache.Remove($"refresh_token_{existingToken}");
            _cache.Remove($"user_refresh_{userId}");
        }
    }

    public async Task<List<UserSummaryDto>> GetAllUsersAsync()
    {
        var users = await _repo.GetAllUsersAsync();
        return users.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null)
            return (false, "User not found.");

        user.IsActive = !user.IsActive;
        await _repo.SaveChangesAsync();

        var status = user.IsActive ? "activated" : "deactivated";
        _logger.LogInformation("User {Id} {Status}", userId, status);
        return (true, $"User {status} successfully.");
    }

    public async Task<(bool Success, string? Data, string Message)> GetUserEmailAsync(int authUserId)
    {
        var user = await _repo.GetByIdAsync(authUserId);
        if (user == null)
            return (false, null, "User not found.");

        return (true, user.Email, "Email fetched successfully.");
    }
}