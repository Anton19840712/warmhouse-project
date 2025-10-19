namespace TelemetryService.Models;

public class TelemetryReading
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = "C";
    public string Status { get; set; } = "active";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? SensorType { get; set; }
}
