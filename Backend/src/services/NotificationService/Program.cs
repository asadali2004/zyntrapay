using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NotificationService.Data;
using NotificationService.Extensions;
using NotificationService.Models;
using NotificationService.Middleware;
using Serilog;
using Shared.Events;

// Configures NotificationService hosting, validation, middleware pipeline, and startup migrations.
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File("logs/notificationservice-.txt", rollingInterval: RollingInterval.Day);
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddEmailSettings(builder.Configuration);
builder.Services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();
builder.Services.AddSingleton<IValidateOptions<RabbitMqSettings>, RabbitMqSettingsValidator>();
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateOnStart();
builder.Services.AddOptions<RabbitMqSettings>()
    .Bind(builder.Configuration.GetSection("RabbitMQ"))
    .ValidateOnStart();
builder.Services.AddOptions<RabbitMqConnectionOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
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

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
