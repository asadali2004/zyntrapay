using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Events;
using System.Text;
using WalletService.Data;
using WalletService.Repositories;
using WalletService.Services;

namespace WalletService.Extensions;

/// <summary>
/// Provides dependency-injection extension methods for WalletService infrastructure wiring.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers WalletService database context and SQL Server provider configuration.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<WalletDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("WalletDB")));
        return services;
    }

    /// <summary>
    /// Registers JWT bearer authentication and token validation rules.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var jwtConfig = config.GetSection("JwtSettings");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!))
                };
            });
        return services;
    }

    /// <summary>
    /// Registers core wallet services, repositories, messaging, and caching dependencies.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<RabbitMqConnectionOptions>()
            .Bind(config.GetSection("RabbitMQ"));
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletServiceImpl>();
        services.AddSingleton<IRabbitMqConnectionFactoryBuilder, RabbitMqConnectionFactoryBuilderImpl>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Registers Swagger/OpenAPI metadata and JWT bearer security definition.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "WalletService API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
                Array.Empty<string>()
            }});
        });
        return services;
    }
}