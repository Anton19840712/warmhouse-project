using Microsoft.EntityFrameworkCore;
using TelemetryService.Data;
using TelemetryService.Models;

namespace TelemetryService.Endpoints;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/telemetry").WithTags("Telemetry");

        // POST /api/telemetry - Save telemetry reading
        group.MapPost("/", async (TelemetryReading reading, TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Saving telemetry reading for sensor {SensorId}", reading.SensorId);

            db.TelemetryReadings.Add(reading);
            await db.SaveChangesAsync();

            logger.LogInformation("Telemetry reading saved with ID {Id}", reading.Id);
            return Results.Created($"/api/telemetry/{reading.Id}", reading);
        })
        .WithName("SaveTelemetry")
        .WithOpenApi();

        // GET /api/telemetry/{sensorId} - Get all readings for a sensor
        group.MapGet("/{sensorId:int}", async (int sensorId, TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching telemetry for sensor {SensorId}", sensorId);

            var readings = await db.TelemetryReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .Take(100)
                .ToListAsync();

            return Results.Ok(readings);
        })
        .WithName("GetTelemetryBySensorId")
        .WithOpenApi();

        // GET /api/telemetry/{sensorId}/latest - Get latest reading
        group.MapGet("/{sensorId:int}/latest", async (int sensorId, TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching latest telemetry for sensor {SensorId}", sensorId);

            var reading = await db.TelemetryReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();

            return reading is not null ? Results.Ok(reading) : Results.NotFound();
        })
        .WithName("GetLatestTelemetry")
        .WithOpenApi();

        // GET /api/telemetry/{sensorId}/statistics - Get statistics
        group.MapGet("/{sensorId:int}/statistics", async (int sensorId, TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Calculating statistics for sensor {SensorId}", sensorId);

            var readings = await db.TelemetryReadings
                .Where(r => r.SensorId == sensorId)
                .Select(r => r.Value)
                .ToListAsync();

            if (!readings.Any())
            {
                return Results.NotFound();
            }

            var stats = new
            {
                sensorId,
                count = readings.Count,
                average = Math.Round(readings.Average(), 2),
                min = Math.Round(readings.Min(), 2),
                max = Math.Round(readings.Max(), 2),
                latest = Math.Round(readings.First(), 2)
            };

            return Results.Ok(stats);
        })
        .WithName("GetTelemetryStatistics")
        .WithOpenApi();
    }
}
