using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WalletService.Data;
using WalletService.Extensions;
using WalletService.Middleware;

// Configures WalletService hosting, dependency injection, middleware pipeline, and startup database checks.
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File("logs/walletservice-.txt", rollingInterval: RollingInterval.Day);
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var firstError = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault();

        return new BadRequestObjectResult(new
        {
            message = string.IsNullOrWhiteSpace(firstError) ? "Validation failed." : firstError,
            errorCode = "VALIDATION_FAILED"
        });
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('Wallets', 'UserEmail') IS NULL
            BEGIN
                ALTER TABLE [Wallets]
                ADD [UserEmail] nvarchar(150) NOT NULL
                    CONSTRAINT [DF_Wallets_UserEmail] DEFAULT '';
            END
            """);
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WalletService v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>
/// Exposes the entry point type for integration testing.
/// </summary>
public partial class Program { }