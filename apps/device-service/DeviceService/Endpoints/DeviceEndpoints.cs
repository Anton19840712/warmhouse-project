using Microsoft.EntityFrameworkCore;
using DeviceService.Data;
using DeviceService.Models;

namespace DeviceService.Endpoints;

public static class DeviceEndpoints
{
    public static void MapDeviceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/devices").WithTags("Devices");

        // GET /api/devices - Get all devices
        group.MapGet("/", async (DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching all devices");
            var devices = await db.Devices.ToListAsync();
            return Results.Ok(devices);
        })
        .WithName("GetAllDevices")
        .WithOpenApi();

        // GET /api/devices/{id} - Get device by ID
        group.MapGet("/{id:int}", async (int id, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching device {DeviceId}", id);
            var device = await db.Devices.FindAsync(id);
            return device is not null ? Results.Ok(device) : Results.NotFound();
        })
        .WithName("GetDeviceById")
        .WithOpenApi();

        // POST /api/devices - Create new device
        group.MapPost("/", async (Device device, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating device: {DeviceName}", device.Name);

            db.Devices.Add(device);
            await db.SaveChangesAsync();

            logger.LogInformation("Device created with ID {DeviceId}", device.Id);
            return Results.Created($"/api/devices/{device.Id}", device);
        })
        .WithName("CreateDevice")
        .WithOpenApi();

        // PUT /api/devices/{id} - Update device
        group.MapPut("/{id:int}", async (int id, Device updatedDevice, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Updating device {DeviceId}", id);

            var device = await db.Devices.FindAsync(id);
            if (device is null)
            {
                return Results.NotFound();
            }

            device.Name = updatedDevice.Name;
            device.Type = updatedDevice.Type;
            device.Location = updatedDevice.Location;
            device.Status = updatedDevice.Status;
            device.SerialNumber = updatedDevice.SerialNumber;

            await db.SaveChangesAsync();

            logger.LogInformation("Device {DeviceId} updated", id);
            return Results.Ok(device);
        })
        .WithName("UpdateDevice")
        .WithOpenApi();

        // DELETE /api/devices/{id} - Delete device
        group.MapDelete("/{id:int}", async (int id, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Deleting device {DeviceId}", id);

            var device = await db.Devices.FindAsync(id);
            if (device is null)
            {
                return Results.NotFound();
            }

            db.Devices.Remove(device);
            await db.SaveChangesAsync();

            logger.LogInformation("Device {DeviceId} deleted", id);
            return Results.NoContent();
        })
        .WithName("DeleteDevice")
        .WithOpenApi();

        // POST /api/devices/{id}/command - Send command to device
        group.MapPost("/{id:int}/command", async (int id, DeviceCommand command, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Sending command '{Command}' to device {DeviceId}", command.Command, id);

            var device = await db.Devices.FindAsync(id);
            if (device is null)
            {
                return Results.NotFound(new { error = "Device not found" });
            }

            command.DeviceId = id;
            command.CreatedAt = DateTime.UtcNow;
            command.Status = "pending";

            db.DeviceCommands.Add(command);

            // Simulate command execution
            device.LastCommandAt = DateTime.UtcNow;
            if (command.Command.ToLower() == "turn_on")
            {
                device.Status = "active";
            }
            else if (command.Command.ToLower() == "turn_off")
            {
                device.Status = "inactive";
            }

            await db.SaveChangesAsync();

            // Mark command as executed
            command.Status = "executed";
            command.ExecutedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            logger.LogInformation("Command executed for device {DeviceId}", id);
            return Results.Ok(new {
                message = "Command executed successfully",
                command = command,
                device = device
            });
        })
        .WithName("SendDeviceCommand")
        .WithOpenApi();

        // GET /api/devices/{id}/commands - Get device command history
        group.MapGet("/{id:int}/commands", async (int id, DeviceContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Fetching command history for device {DeviceId}", id);

            var commands = await db.DeviceCommands
                .Where(c => c.DeviceId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Results.Ok(commands);
        })
        .WithName("GetDeviceCommands")
        .WithOpenApi();
    }
}
