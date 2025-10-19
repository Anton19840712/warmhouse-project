using Microsoft.EntityFrameworkCore;
using TelemetryService.Data;
using TelemetryService.Models;

namespace TelemetryService.Tests;

public class TelemetryEndpointsTests
{
    private TelemetryContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TelemetryContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TelemetryContext(options);
    }

    [Fact]
    public async Task SaveReading_ShouldAddReadingToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var reading = new TelemetryReading
        {
            SensorId = 1,
            Value = 22.5,
            Unit = "C",
            Status = "active",
            Timestamp = DateTime.UtcNow,
            Location = "Living Room",
            SensorType = "temperature"
        };

        // Act
        context.TelemetryReadings.Add(reading);
        await context.SaveChangesAsync();

        // Assert
        var savedReading = await context.TelemetryReadings.FirstOrDefaultAsync();
        Assert.NotNull(savedReading);
        Assert.Equal(1, savedReading.SensorId);
        Assert.Equal(22.5, savedReading.Value);
        Assert.Equal("C", savedReading.Unit);
    }

    [Fact]
    public async Task GetReadings_ShouldReturnReadingsForSensor()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var readings = new[]
        {
            new TelemetryReading { SensorId = 1, Value = 20.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new TelemetryReading { SensorId = 1, Value = 21.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new TelemetryReading { SensorId = 1, Value = 22.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow },
            new TelemetryReading { SensorId = 2, Value = 18.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow }
        };

        context.TelemetryReadings.AddRange(readings);
        await context.SaveChangesAsync();

        // Act
        var sensor1Readings = await context.TelemetryReadings
            .Where(r => r.SensorId == 1)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

        // Assert
        Assert.Equal(3, sensor1Readings.Count);
        Assert.Equal(22.0, sensor1Readings[0].Value); // Most recent
    }

    [Fact]
    public async Task GetLatestReading_ShouldReturnMostRecentReading()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var readings = new[]
        {
            new TelemetryReading { SensorId = 1, Value = 20.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow.AddHours(-2) },
            new TelemetryReading { SensorId = 1, Value = 21.0, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow.AddHours(-1) },
            new TelemetryReading { SensorId = 1, Value = 22.5, Unit = "C", Status = "active", Timestamp = DateTime.UtcNow }
        };

        context.TelemetryReadings.AddRange(readings);
        await context.SaveChangesAsync();

        // Act
        var latestReading = await context.TelemetryReadings
            .Where(r => r.SensorId == 1)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(latestReading);
        Assert.Equal(22.5, latestReading.Value);
    }

    [Fact]
    public async Task GetStatistics_ShouldCalculateCorrectAverageMinMax()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var now = DateTime.UtcNow;
        var readings = new[]
        {
            new TelemetryReading { SensorId = 1, Value = 18.0, Unit = "C", Status = "active", Timestamp = now.AddHours(-3) },
            new TelemetryReading { SensorId = 1, Value = 20.0, Unit = "C", Status = "active", Timestamp = now.AddHours(-2) },
            new TelemetryReading { SensorId = 1, Value = 22.0, Unit = "C", Status = "active", Timestamp = now.AddHours(-1) },
            new TelemetryReading { SensorId = 1, Value = 24.0, Unit = "C", Status = "active", Timestamp = now }
        };

        context.TelemetryReadings.AddRange(readings);
        await context.SaveChangesAsync();

        // Act
        var stats = await context.TelemetryReadings
            .Where(r => r.SensorId == 1)
            .GroupBy(r => r.SensorId)
            .Select(g => new
            {
                Average = g.Average(r => r.Value),
                Min = g.Min(r => r.Value),
                Max = g.Max(r => r.Value),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(21.0, stats.Average); // (18 + 20 + 22 + 24) / 4
        Assert.Equal(18.0, stats.Min);
        Assert.Equal(24.0, stats.Max);
        Assert.Equal(4, stats.Count);
    }

    [Fact]
    public async Task TelemetryReading_ShouldHaveDefaultTimestamp()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = 1,
            Value = 20.0,
            Unit = "C",
            Status = "active"
        };

        // Assert
        Assert.NotEqual(default(DateTime), reading.Timestamp);
        Assert.InRange(reading.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task TelemetryReading_ShouldHaveDefaultUnit()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = 1,
            Value = 20.0,
            Status = "active"
        };

        // Assert
        Assert.Equal("C", reading.Unit);
    }

    [Fact]
    public async Task TelemetryReading_ShouldHaveDefaultStatus()
    {
        // Arrange
        var reading = new TelemetryReading
        {
            SensorId = 1,
            Value = 20.0
        };

        // Assert
        Assert.Equal("active", reading.Status);
    }
}
