using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}

public class AuthServiceImpl : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly JwtSettings _jwt;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServiceImpl> _logger;

    public AuthServiceImpl(
        IAuthRepository repo,
        IOptions<JwtSettings> jwt,
        IConfiguration configuration,
        ILogger<AuthServiceImpl> logger)
    {
        _repo = repo;
        _jwt = jwt.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto)
    {
        _logger.LogInformation("Register attempt for email: {Email}", dto.Email); 

        if (await _repo.EmailExistsAsync(dto.Email))
            return (false, "Email already registered.");

        if (await _repo.PhoneExistsAsync(dto.PhoneNumber))
            return (false, "Phone number already registered.");

        var user = new User
        {
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddUserAsync(user);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("User registered successfully: {Email}", dto.Email); 
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

        var user = await _repo.GetByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email); 
            return (false, null, "Invalid email or password.");
        }

        if (!user.IsActive)
            return (false, null, "Account is deactivated.");

        var token = GenerateToken(user);

        _logger.LogInformation("Login successful for email: {Email}", dto.Email); 
        return (true, new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role
        }, "Login successful.");
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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
}