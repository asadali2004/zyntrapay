using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day);
});

// Load ocelot.json on top of appsettings.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

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

app.MapHealthChecks("/health");
app.UseCors("AllowAngular");

// Ocelot must be last — it handles all routing
await app.UseOcelot();

app.Run();