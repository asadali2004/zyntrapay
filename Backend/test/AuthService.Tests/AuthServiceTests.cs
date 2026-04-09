using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shared.Events;

namespace AuthService.Tests;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAuthRepository> _repoMock = null!;
    private Mock<ILogger<AuthServiceImpl>> _loggerMock = null!;
    private Mock<IConfiguration> _configMock = null!;
    private Mock<IRabbitMqPublisher> _publisherMock = null!;
    private IMemoryCache _cache = null!;
    private AuthServiceImpl _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IAuthRepository>();
        _loggerMock = new Mock<ILogger<AuthServiceImpl>>();
        _configMock = new Mock<IConfiguration>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _publisherMock.Setup(p => p.Publish(It.IsAny<object>())).Returns(true);
        _cache = new MemoryCache(new MemoryCacheOptions());

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TestSuperSecretKey12345678901234",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        });

        _configMock.Setup(c => c["AdminSettings:SecretKey"])
            .Returns("AdminSecret@2024");

        _authService = new AuthServiceImpl(
            _repoMock.Object,
            jwtSettings,
            _configMock.Object,
            _loggerMock.Object,
            _cache,
            _publisherMock.Object
        );
    }

    // ── REGISTER TESTS ──────────────────────────────────────────────────────

    [Test]
    public async Task Register_WithUnverifiedEmail_ReturnsFalse()
    {
        // Arrange — no OTP verified in cache
        var dto = new RegisterRequestDto
        {
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Password = "Test@123"
        };

        // Act
        var (success, message) = await _authService.RegisterAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("not verified"));
    }

    [Test]
    public async Task Register_WithVerifiedEmail_Success()
    {
        // Arrange — mark email as verified in cache
        _cache.Set("verified_john@example.com", true, TimeSpan.FromMinutes(15));

        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(false);
        _repoMock.Setup(r => r.PhoneExistsAsync("9876543210")).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new RegisterRequestDto
        {
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Password = "Test@123"
        };

        // Act
        var (success, message) = await _authService.RegisterAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("successful"));
    }

    [Test]
    public async Task Register_WhenWelcomeEmailPublishFails_ReturnsSuccessWithWarning()
    {
        _cache.Set("verified_john@example.com", true, TimeSpan.FromMinutes(15));

        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(false);
        _repoMock.Setup(r => r.PhoneExistsAsync("9876543210")).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.Publish(It.IsAny<WelcomeEmailRequestedEvent>())).Returns(false);

        var dto = new RegisterRequestDto
        {
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Password = "Test@123"
        };

        var (success, message) = await _authService.RegisterAsync(dto);

        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("welcome email could not be queued"));
    }

    [Test]
    public async Task Register_WithDuplicateEmail_ReturnsFalse()
    {
        // Arrange
        _cache.Set("verified_john@example.com", true, TimeSpan.FromMinutes(15));
        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(true);

        var dto = new RegisterRequestDto
        {
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Password = "Test@123"
        };

        // Act
        var (success, message) = await _authService.RegisterAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Email already registered"));
    }

    [Test]
    public async Task Register_WithDuplicatePhone_ReturnsFalse()
    {
        // Arrange
        _cache.Set("verified_john@example.com", true, TimeSpan.FromMinutes(15));
        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(false);
        _repoMock.Setup(r => r.PhoneExistsAsync("9876543210")).ReturnsAsync(true);

        var dto = new RegisterRequestDto
        {
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Password = "Test@123"
        };

        // Act
        var (success, message) = await _authService.RegisterAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Phone number already registered"));
    }

    // ── LOGIN TESTS ──────────────────────────────────────────────────────────

    [Test]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            PasswordHash = hashedPassword,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var dto = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "Test@123"
        };

        // Act
        var (success, data, message) = await _authService.LoginAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Token, Is.Not.Empty);
        Assert.That(data.Role, Is.EqualTo("User"));
    }

    [Test]
    public async Task Login_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PasswordHash = hashedPassword,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var dto = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "WrongPassword"
        };

        // Act
        var (success, data, message) = await _authService.LoginAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("Invalid"));
    }

    [Test]
    public async Task Login_WithNonExistentEmail_ReturnsFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync("nobody@example.com"))
            .ReturnsAsync((User?)null);

        var dto = new LoginRequestDto
        {
            Email = "nobody@example.com",
            Password = "Test@123"
        };

        // Act
        var (success, data, message) = await _authService.LoginAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
    }

    [Test]
    public async Task Login_WithDeactivatedAccount_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = "User",
            IsActive = false,  // deactivated
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);

        var dto = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "Test@123"
        };

        // Act
        var (success, data, message) = await _authService.LoginAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("deactivated"));
    }

    // ── ADMIN REGISTER TESTS ──────────────────────────────────────────────

    [Test]
    public async Task RegisterAdmin_WithCorrectSecretKey_Success()
    {
        // Arrange
        _repoMock.Setup(r => r.EmailExistsAsync("admin@zyntrapay.com")).ReturnsAsync(false);
        _repoMock.Setup(r => r.PhoneExistsAsync("9000000001")).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new AdminRegisterRequestDto
        {
            Email = "admin@zyntrapay.com",
            PhoneNumber = "9000000001",
            Password = "Admin@123",
            AdminSecretKey = "AdminSecret@2024"
        };

        // Act
        var (success, message) = await _authService.RegisterAdminAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("Admin registered"));
    }

    [Test]
    public async Task RegisterAdmin_WithWrongSecretKey_ReturnsFalse()
    {
        // Arrange
        var dto = new AdminRegisterRequestDto
        {
            Email = "admin@zyntrapay.com",
            PhoneNumber = "9000000001",
            Password = "Admin@123",
            AdminSecretKey = "WrongKey"
        };

        // Act
        var (success, message) = await _authService.RegisterAdminAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Invalid admin secret key"));
    }

    // ── OTP TESTS ────────────────────────────────────────────────────────────

    [Test]
    public async Task VerifyOtp_WithCorrectOtp_ReturnsTrue()
    {
        // Arrange — manually set OTP in cache
        _cache.Set("otp_john@example.com", "123456", TimeSpan.FromMinutes(10));

        var dto = new VerifyOtpRequestDto
        {
            Email = "john@example.com",
            Otp = "123456"
        };

        // Act
        var (success, message) = await _authService.VerifyOtpAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("verified"));
    }

    [Test]
    public async Task SendOtp_WhenPublishFails_ReturnsFalseAndClearsOtp()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(false);

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<OtpRequestedEvent>()))
            .Returns(false);

        var dto = new SendOtpRequestDto
        {
            Email = "john@example.com"
        };

        var (success, message) = await _authService.SendOtpAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Unable to send OTP"));
        Assert.That(_cache.TryGetValue("otp_john@example.com", out _), Is.False);
    }

    [Test]
    public async Task SendOtp_WithExistingEmail_ReturnsFalseWithoutPublish()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(true);

        var dto = new SendOtpRequestDto
        {
            Email = "john@example.com"
        };

        var (success, message) = await _authService.SendOtpAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Email already registered"));
        _publisherMock.Verify(p => p.Publish(It.IsAny<OtpRequestedEvent>()), Times.Never);
        Assert.That(_cache.TryGetValue("otp_john@example.com", out _), Is.False);
    }

    [Test]
    public async Task VerifyOtp_WithWrongOtp_ReturnsFalse()
    {
        // Arrange
        _cache.Set("otp_john@example.com", "123456", TimeSpan.FromMinutes(10));

        var dto = new VerifyOtpRequestDto
        {
            Email = "john@example.com",
            Otp = "999999"
        };

        // Act
        var (success, message) = await _authService.VerifyOtpAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Invalid OTP"));
    }

    [Test]
    public async Task VerifyOtp_WithExpiredOtp_ReturnsFalse()
    {
        // Arrange — no OTP in cache (simulates expiry)
        var dto = new VerifyOtpRequestDto
        {
            Email = "john@example.com",
            Otp = "123456"
        };

        // Act
        var (success, message) = await _authService.VerifyOtpAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("expired"));
    }

    [Test]
    public async Task VerifyOtp_WithExistingEmail_ReturnsFalse()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("john@example.com")).ReturnsAsync(true);
        _cache.Set("otp_john@example.com", "123456", TimeSpan.FromMinutes(10));

        var dto = new VerifyOtpRequestDto
        {
            Email = "john@example.com",
            Otp = "123456"
        };

        var (success, message) = await _authService.VerifyOtpAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Email already registered"));
    }

    // ── FORGOT/RESET PASSWORD TESTS ───────────────────────────────────────

    [Test]
    public async Task ForgotPassword_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(new User
        {
            Id = 1,
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        var dto = new ForgotPasswordRequestDto { Email = "john@example.com" };

        // Act
        var (success, message) = await _authService.ForgotPasswordAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("reset OTP").IgnoreCase);
        _publisherMock.Verify(p => p.Publish(It.IsAny<OtpRequestedEvent>()), Times.Once);
    }

    [Test]
    public async Task ForgotPassword_WhenPublishFails_ReturnsFalseAndClearsOtp()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(new User
        {
            Id = 1,
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<OtpRequestedEvent>()))
            .Returns(false);

        var dto = new ForgotPasswordRequestDto { Email = "john@example.com" };

        var (success, message) = await _authService.ForgotPasswordAsync(dto);

        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Unable to send reset OTP"));
        Assert.That(_cache.TryGetValue("reset_otp_john@example.com", out _), Is.False);
    }

    [Test]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsTrueWithoutPublish()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync("nobody@example.com"))
            .ReturnsAsync((User?)null);

        var dto = new ForgotPasswordRequestDto { Email = "nobody@example.com" };

        // Act
        var (success, message) = await _authService.ForgotPasswordAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("If this email is registered"));
        _publisherMock.Verify(p => p.Publish(It.IsAny<OtpRequestedEvent>()), Times.Never);
    }

    [Test]
    public async Task ResetPassword_WithExpiredOtp_ReturnsFalse()
    {
        // Arrange
        var dto = new ResetPasswordRequestDto
        {
            Email = "john@example.com",
            Otp = "123456",
            NewPassword = "NewPass123"
        };

        // Act
        var (success, message) = await _authService.ResetPasswordAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("expired or not found"));
    }

    [Test]
    public async Task ResetPassword_WithWrongOtp_ReturnsFalse()
    {
        // Arrange
        _cache.Set("reset_otp_john@example.com", "123456", TimeSpan.FromMinutes(10));

        var dto = new ResetPasswordRequestDto
        {
            Email = "john@example.com",
            Otp = "999999",
            NewPassword = "NewPass123"
        };

        // Act
        var (success, message) = await _authService.ResetPasswordAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(message, Does.Contain("Invalid OTP"));
    }

    [Test]
    public async Task ResetPassword_WithValidOtp_UpdatesPassword()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123"),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set("reset_otp_john@example.com", "123456", TimeSpan.FromMinutes(10));
        _repoMock.Setup(r => r.GetByEmailAsync("john@example.com")).ReturnsAsync(user);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new ResetPasswordRequestDto
        {
            Email = "john@example.com",
            Otp = "123456",
            NewPassword = "NewPass123"
        };

        // Act
        var (success, message) = await _authService.ResetPasswordAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(message, Does.Contain("successful"));
        Assert.That(BCrypt.Net.BCrypt.Verify("NewPass123", user.PasswordHash), Is.True);
    }

    // ── REFRESH TOKEN TESTS ───────────────────────────────────────────────

    [Test]
    public async Task RefreshToken_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var dto = new RefreshTokenRequestDto { RefreshToken = "invalid_token" };

        // Act
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("Invalid or expired refresh token"));
    }

    [Test]
    public async Task RefreshToken_WithValidToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set("refresh_token_test_refresh", 1, TimeSpan.FromMinutes(10));
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var dto = new RefreshTokenRequestDto { RefreshToken = "test_refresh" };

        // Act
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Token, Is.Not.Empty);
        Assert.That(data.RefreshToken, Is.Not.Empty);
        Assert.That(message, Does.Contain("refreshed"));
    }

    [Test]
    public async Task RefreshToken_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Role = "User",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set("refresh_token_test_refresh", 1, TimeSpan.FromMinutes(10));
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var dto = new RefreshTokenRequestDto { RefreshToken = "test_refresh" };

        // Act
        var (success, data, message) = await _authService.RefreshTokenAsync(dto);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(data, Is.Null);
        Assert.That(message, Does.Contain("inactive"));
    }
}
