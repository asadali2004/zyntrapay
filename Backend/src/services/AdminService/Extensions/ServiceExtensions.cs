using AdminService.Data;
using AdminService.Repositories;
using AdminService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Shared.Events;
using System.Net.Http;
using System.Text;

namespace AdminService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AdminDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("AdminDB")));
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
     this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<RabbitMqConnectionOptions>()
            .Bind(config.GetSection("RabbitMQ"));
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminService, AdminServiceImpl>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Register IHttpContextAccessor and handler
        services.AddHttpContextAccessor();
        services.AddTransient<AuthTokenHandler>();

        // HttpClient for UserService — with token forwarding + resilience
        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            client.BaseAddress = new Uri(config["ServiceUrls:UserService"]!);
        })
        .AddHttpMessageHandler<AuthTokenHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        // HttpClient for AuthService — with token forwarding + resilience
        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
        {
            client.BaseAddress = new Uri(config["ServiceUrls:AuthService"]!);
        })
        .AddHttpMessageHandler<AuthTokenHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        => Policy.TimeoutAsync<HttpResponseMessage>(10);

    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AdminService API",
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
