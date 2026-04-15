using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

// Configures API Gateway hosting, Ocelot routing, CORS policy, and health endpoint.
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day);
});

// Load base Ocelot config and optionally override it per environment (for Docker/local).
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Register Ocelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddHealthChecks();

// CORS — allows Angular (port 4200) to call the gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");
app.UseHealthChecks("/health");

// Ocelot must be last — it handles all routing
await app.UseOcelot();

app.Run();