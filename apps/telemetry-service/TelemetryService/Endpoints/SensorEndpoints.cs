using Microsoft.EntityFrameworkCore;
using TelemetryService.Data;

namespace TelemetryService.Endpoints;

public static class SensorEndpoints
{
    public static void MapSensorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sensors").WithTags("Sensors");

        group.MapGet("/", async (TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching all sensors");

            var sensors = await db.Sensors
                .OrderBy(s => s.Id)
                .ToListAsync();

            return Results.Ok(sensors);
        })
        .WithName("GetAllSensors")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, TelemetryContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching sensor {SensorId}", id);

            var sensor = await db.Sensors.FindAsync(id);

            return sensor is not null ? Results.Ok(sensor) : Results.NotFound();
        })
        .WithName("GetSensorById")
        .WithOpenApi();
    }
}
