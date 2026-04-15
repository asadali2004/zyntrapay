using UserService.Data;
using UserService.Repositories;
using UserService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace UserService.Extensions;

/// <summary>
/// Provides dependency-injection extension methods for UserService infrastructure wiring.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers UserService database context and SQL Server provider configuration.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("UserDB")));
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
    /// Registers core application services and repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserServiceImpl>();
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
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AuthService API",
                Version = "v1"
            });
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
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }
}