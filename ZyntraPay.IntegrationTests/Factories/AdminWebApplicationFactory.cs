using AdminService.Data;
using AdminService.DTOs;
using AdminService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ZyntraPay.IntegrationTests.Factories;

public class AdminWebApplicationFactory : WebApplicationFactory<AdminServiceImpl>
{
    private readonly string _databaseName = $"AdminTestDB_{Guid.NewGuid()}";

    public AdminDownstreamData Data { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKey12345678901234",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["ServiceUrls:UserService"] = "http://localhost:5005",
                ["ServiceUrls:AuthService"] = "http://localhost:5003"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AdminDbContext>>();
            services.RemoveAll<AdminDbContext>();
            services.AddDbContext<AdminDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IUserServiceClient>();
            services.RemoveAll<IAuthServiceClient>();
            services.RemoveAll<IRabbitMqPublisher>();

            services.AddSingleton(Data);
            services.AddSingleton<IUserServiceClient, FakeUserServiceClient>();
            services.AddSingleton<IAuthServiceClient, FakeAuthServiceClient>();

            var mockPublisher = new Mock<IRabbitMqPublisher>();
            mockPublisher.Setup(p => p.Publish(It.IsAny<object>()));
            services.AddSingleton(mockPublisher.Object);

            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "TestIssuer",
                        ValidAudience = "TestAudience",
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKey12345678901234"))
                    };
                });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}

public class AdminDownstreamData
{
    public List<KycSubmissionDto> PendingKycs { get; set; } = new();
    public List<UserSummaryDto> Users { get; set; } = new();
    public bool ReviewKycShouldSucceed { get; set; } = true;
    public bool ToggleUserShouldSucceed { get; set; } = true;
}

internal class FakeUserServiceClient : IUserServiceClient
{
    private readonly AdminDownstreamData _data;

    public FakeUserServiceClient(AdminDownstreamData data)
    {
        _data = data;
    }

    public Task<List<KycSubmissionDto>> GetPendingKycsAsync()
        => Task.FromResult(_data.PendingKycs);

    public Task<bool> ReviewKycAsync(int kycId, ReviewKycDto dto)
        => Task.FromResult(_data.ReviewKycShouldSucceed);

    public Task<KycSubmissionDto?> GetKycByIdAsync(int kycId)
        => Task.FromResult(_data.PendingKycs.FirstOrDefault(k => k.Id == kycId));
}

internal class FakeAuthServiceClient : IAuthServiceClient
{
    private readonly AdminDownstreamData _data;

    public FakeAuthServiceClient(AdminDownstreamData data)
    {
        _data = data;
    }

    public Task<List<UserSummaryDto>> GetAllUsersAsync()
        => Task.FromResult(_data.Users);

    public Task<bool> ToggleUserStatusAsync(int userId)
        => Task.FromResult(_data.ToggleUserShouldSucceed);

    public Task<string?> GetUserEmailAsync(int authUserId)
        => Task.FromResult(_data.Users.FirstOrDefault(u => u.Id == authUserId)?.Email);
}
