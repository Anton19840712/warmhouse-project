using Microsoft.EntityFrameworkCore;
using DeviceService.Data;
using DeviceService.Models;

namespace DeviceService.Tests;

public class DeviceEndpointsTests
{
    private DeviceContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DeviceContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DeviceContext(options);
    }

    [Fact]
    public async Task CreateDevice_ShouldAddDeviceToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "Smart Thermostat",
            Type = "thermostat",
            Location = "Living Room",
            Status = "active",
            SerialNumber = "ST-001"
        };

        // Act
        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // Assert
        var savedDevice = await context.Devices.FirstOrDefaultAsync();
        Assert.NotNull(savedDevice);
        Assert.Equal("Smart Thermostat", savedDevice.Name);
        Assert.Equal("thermostat", savedDevice.Type);
        Assert.Equal("Living Room", savedDevice.Location);
        Assert.Equal("active", savedDevice.Status);
        Assert.Equal("ST-001", savedDevice.SerialNumber);
    }

    [Fact]
    public async Task GetAllDevices_ShouldReturnAllDevices()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var devices = new[]
        {
            new Device { Name = "Thermostat 1", Type = "thermostat", Location = "Living Room", Status = "active" },
            new Device { Name = "Thermostat 2", Type = "thermostat", Location = "Bedroom", Status = "inactive" },
            new Device { Name = "Light 1", Type = "light", Location = "Kitchen", Status = "active" }
        };

        context.Devices.AddRange(devices);
        await context.SaveChangesAsync();

        // Act
        var allDevices = await context.Devices.ToListAsync();

        // Assert
        Assert.Equal(3, allDevices.Count);
    }

    [Fact]
    public async Task GetDeviceById_ShouldReturnCorrectDevice()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "Smart Thermostat",
            Type = "thermostat",
            Location = "Living Room",
            Status = "active"
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // Act
        var foundDevice = await context.Devices.FindAsync(device.Id);

        // Assert
        Assert.NotNull(foundDevice);
        Assert.Equal("Smart Thermostat", foundDevice.Name);
    }

    [Fact]
    public async Task UpdateDevice_ShouldModifyDevice()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "Old Name",
            Type = "thermostat",
            Location = "Living Room",
            Status = "inactive"
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // Act
        device.Name = "New Name";
        device.Status = "active";
        await context.SaveChangesAsync();

        // Assert
        var updatedDevice = await context.Devices.FindAsync(device.Id);
        Assert.Equal("New Name", updatedDevice!.Name);
        Assert.Equal("active", updatedDevice.Status);
    }

    [Fact]
    public async Task DeleteDevice_ShouldRemoveDeviceFromDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "To Delete",
            Type = "thermostat",
            Location = "Living Room",
            Status = "active"
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // Act
        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        // Assert
        var deletedDevice = await context.Devices.FindAsync(device.Id);
        Assert.Null(deletedDevice);
    }

    [Fact]
    public async Task SendCommand_ShouldCreateDeviceCommand()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "Smart Thermostat",
            Type = "thermostat",
            Location = "Living Room",
            Status = "active"
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        var command = new DeviceCommand
        {
            DeviceId = device.Id,
            Command = "setTemperature",
            Parameters = "22",
            Status = "pending"
        };

        // Act
        context.DeviceCommands.Add(command);
        await context.SaveChangesAsync();

        // Assert
        var savedCommand = await context.DeviceCommands.FirstOrDefaultAsync();
        Assert.NotNull(savedCommand);
        Assert.Equal(device.Id, savedCommand.DeviceId);
        Assert.Equal("setTemperature", savedCommand.Command);
        Assert.Equal("22", savedCommand.Parameters);
    }

    [Fact]
    public async Task GetDeviceCommands_ShouldReturnCommandsForDevice()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var device = new Device
        {
            Name = "Smart Thermostat",
            Type = "thermostat",
            Location = "Living Room",
            Status = "active"
        };

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        var commands = new[]
        {
            new DeviceCommand { DeviceId = device.Id, Command = "setTemperature", Parameters = "20", Status = "executed" },
            new DeviceCommand { DeviceId = device.Id, Command = "setTemperature", Parameters = "22", Status = "executed" },
            new DeviceCommand { DeviceId = device.Id, Command = "setMode", Parameters = "heat", Status = "pending" }
        };

        context.DeviceCommands.AddRange(commands);
        await context.SaveChangesAsync();

        // Act
        var deviceCommands = await context.DeviceCommands
            .Where(c => c.DeviceId == device.Id)
            .ToListAsync();

        // Assert
        Assert.Equal(3, deviceCommands.Count);
    }

    [Fact]
    public async Task Device_ShouldHaveDefaultStatus()
    {
        // Arrange
        var device = new Device
        {
            Name = "Test Device",
            Type = "thermostat",
            Location = "Test Location"
        };

        // Assert
        Assert.Equal("inactive", device.Status);
    }

    [Fact]
    public async Task Device_ShouldHaveCreatedAtTimestamp()
    {
        // Arrange
        var device = new Device
        {
            Name = "Test Device",
            Type = "thermostat",
            Location = "Test Location",
            Status = "active"
        };

        // Assert
        Assert.NotEqual(default(DateTime), device.CreatedAt);
        Assert.InRange(device.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task DeviceCommand_ShouldHaveDefaultPendingStatus()
    {
        // Arrange
        var command = new DeviceCommand
        {
            DeviceId = 1,
            Command = "test"
        };

        // Assert
        Assert.Equal("pending", command.Status);
    }
}
