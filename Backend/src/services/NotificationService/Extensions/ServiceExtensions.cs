using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.Services;
using System.Text;

namespace NotificationService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("NotificationDB")));
        return services;
    }

    public static IServiceCollection AddEmailSettings(
    this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        return services;
    }

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

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationServiceImpl>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<WalletTopUpNotificationConsumer>();
        services.AddHostedService<WalletTransferNotificationConsumer>();
        services.AddHostedService<KycStatusChangedConsumer>();
        services.AddHostedService<OtpRequestedConsumer>();
        services.AddHostedService<WelcomeEmailConsumer>();
        services.AddHostedService<PointsAwardedNotificationConsumer>();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NotificationService API",
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
            {{
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }});
        });
        return services;
    }
}