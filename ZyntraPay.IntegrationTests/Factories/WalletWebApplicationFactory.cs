using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WalletService.Data;
using WalletService.Services;

namespace ZyntraPay.IntegrationTests.Factories;

public class WalletWebApplicationFactory : WebApplicationFactory<WalletServiceImpl>
{
    private readonly string _databaseName = $"WalletTestDB_{Guid.NewGuid()}_";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKey12345678901234",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real SQL Server DbContext
            services.RemoveAll<DbContextOptions<WalletDbContext>>();
            services.RemoveAll<WalletDbContext>();

            // Replace with in-memory database
            services.AddDbContext<WalletDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Replace RabbitMQ publisher with mock
            services.RemoveAll<IRabbitMqPublisher>();
            var mockPublisher = new Mock<IRabbitMqPublisher>();
            mockPublisher.Setup(p => p.Publish(It.IsAny<object>()));
            services.AddSingleton(mockPublisher.Object);

            // Override JWT settings for testing
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

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}