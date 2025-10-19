using Microsoft.EntityFrameworkCore;
using Serilog;
using DeviceService.Data;
using DeviceService.Endpoints;

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
    c.SwaggerDoc("v1", new() { Title = "Device Management Service API", Version = "v1" });
});

// Add PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DeviceDb")
    ?? "Host=localhost;Port=15439;Database=smarthome;Username=postgres;Password=postgres";

builder.Services.AddDbContext<DeviceContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Device Management Service API v1"));

app.UseSerilogRequestLogging();

// Map endpoints
app.MapDeviceEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "device-management" }))
    .WithName("HealthCheck")
    .WithOpenApi();

Log.Information("Starting Device Management Service on port 8083");
app.Run();
