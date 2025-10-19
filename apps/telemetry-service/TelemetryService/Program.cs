using Microsoft.EntityFrameworkCore;
using Serilog;
using TelemetryService.Data;
using TelemetryService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Telemetry Service API", Version = "v1" });
});

// Add PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("TelemetryDb")
    ?? "Host=localhost;Port=15439;Database=smarthome;Username=postgres;Password=postgres";

builder.Services.AddDbContext<TelemetryContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Telemetry Service API v1"));

app.UseSerilogRequestLogging();

// Map endpoints
app.MapTelemetryEndpoints();
app.MapSensorEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "telemetry" }))
    .WithName("HealthCheck")
    .WithOpenApi();

Log.Information("Starting Telemetry Service on port 8082");
app.Run();
