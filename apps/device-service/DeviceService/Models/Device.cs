namespace DeviceService.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "inactive";
    public string? SerialNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastCommandAt { get; set; }
}

public class DeviceCommand
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string Command { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }

    public Device? Device { get; set; }
}
