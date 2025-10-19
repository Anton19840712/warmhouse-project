namespace TelemetryService.Models;

public class Sensor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Status { get; set; } = "inactive";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
