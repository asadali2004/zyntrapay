using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NotificationService.Data;
using NotificationService.Services;

namespace ZyntraPay.IntegrationTests.Factories;

public class NotificationWebApplicationFactory : WebApplicationFactory<NotificationServiceImpl>
{
    private readonly string _databaseName = $"NotificationTestDB_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSuperSecretKey12345678901234",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["EmailSettings:Host"] = "smtp.test.local",
                ["EmailSettings:Port"] = "2525",
                ["EmailSettings:Username"] = "test-user",
                ["EmailSettings:Password"] = "test-pass",
                ["EmailSettings:FromName"] = "ZyntraPay Test",
                ["EmailSettings:FromEmail"] = "noreply@test.local",
                ["RabbitMQ:Host"] = "localhost",
                ["RabbitMQ:Username"] = "guest",
                ["RabbitMQ:Password"] = "guest"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationDbContext>>();
            services.RemoveAll<NotificationDbContext>();

            services.AddDbContext<NotificationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IHostedService>();

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
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
