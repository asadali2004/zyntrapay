using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ZyntraPay.IntegrationTests.Factories;

public class AuthWebApplicationFactory : WebApplicationFactory<AuthServiceImpl>
{
    private readonly string _databaseName = $"AuthTestDB_{Guid.NewGuid()}";
 
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKey12345678901234",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["AdminSettings:SecretKey"] = "AdminSecret@2024"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real SQL Server DbContext
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();

            // Replace with in-memory database
            services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Replace RabbitMQ publisher with mock
            services.RemoveAll<IRabbitMqPublisher>();
            var mockPublisher = new Mock<IRabbitMqPublisher>();
            mockPublisher.Setup(p => p.Publish(It.IsAny<object>()));
            services.AddSingleton(mockPublisher.Object);

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}