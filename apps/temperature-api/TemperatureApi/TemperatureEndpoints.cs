using Microsoft.AspNetCore.Http.HttpResults;

namespace TemperatureApi;

public static class TemperatureEndpoints
{
    private static readonly Random Random = new();
    private static readonly Dictionary<string, string> LocationToId = new()
    {
        { "Living Room", "1" },
        { "Bedroom", "2" },
        { "Kitchen", "3" }
    };
    private static readonly Dictionary<string, string> IdToLocation =
        LocationToId.ToDictionary(x => x.Value, x => x.Key);

    public static void MapTemperatureEndpoints(this WebApplication app)
    {
        app.MapGet("/temperature", GetByLocation)
            .WithName("GetTemperatureByLocation");

        app.MapGet("/temperature/{id}", GetById)
            .WithName("GetTemperatureById");
    }

    private static IResult GetByLocation(string? location, ILogger<Program> logger)
    {
        if (string.IsNullOrEmpty(location))
        {
            logger.LogWarning("Temperature request with empty location");
            return Results.BadRequest(new { error = "Location is required" });
        }

        logger.LogInformation("Temperature requested for location: {Location}", location);
        var sensorId = LocationToId.GetValueOrDefault(location, "0");
        var temperature = Random.NextDouble() * 30 + 10;

        return Results.Ok(new
        {
            value = Math.Round(temperature, 2),
            unit = "C",
            timestamp = DateTime.UtcNow,
            location,
            status = "active",
            sensor_id = sensorId,
            sensor_type = "temperature",
            description = $"Temperature reading from {location}"
        });
    }

    private static IResult GetById(string id, ILogger<Program> logger)
    {
        logger.LogInformation("Temperature requested for sensor ID: {Id}", id);
        var location = IdToLocation.GetValueOrDefault(id, "Unknown");
        var temperature = Random.NextDouble() * 30 + 10;

        return Results.Ok(new
        {
            value = Math.Round(temperature, 2),
            unit = "C",
            timestamp = DateTime.UtcNow,
            location,
            status = "active",
            sensor_id = id,
            sensor_type = "temperature",
            description = $"Temperature reading from sensor {id}"
        });
    }
}
