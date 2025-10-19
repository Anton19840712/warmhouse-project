using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TemperatureApi.Tests;

public class TemperatureEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TemperatureEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTemperatureByLocation_ValidLocation_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/temperature?location=Living%20Room");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        Assert.NotNull(data);
        Assert.Equal("Living Room", data.location);
        Assert.Equal("1", data.sensor_id);
        Assert.Equal("C", data.unit);
        Assert.Equal("active", data.status);
        Assert.InRange(data.value, 10, 40);
    }

    [Fact]
    public async Task GetTemperatureByLocation_EmptyLocation_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/temperature?location=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTemperatureByLocation_UnknownLocation_ReturnsOkWithDefaultId()
    {
        // Act
        var response = await _client.GetAsync("/temperature?location=Unknown");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        Assert.NotNull(data);
        Assert.Equal("0", data.sensor_id);
    }

    [Fact]
    public async Task GetTemperatureById_ValidId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/temperature/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        Assert.NotNull(data);
        Assert.Equal("Living Room", data.location);
        Assert.Equal("1", data.sensor_id);
        Assert.InRange(data.value, 10, 40);
    }

    [Fact]
    public async Task GetTemperatureById_UnknownId_ReturnsOkWithUnknownLocation()
    {
        // Act
        var response = await _client.GetAsync("/temperature/999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        Assert.NotNull(data);
        Assert.Equal("Unknown", data.location);
    }

    [Fact]
    public async Task GetTemperature_MultipleRequests_ReturnsDifferentValues()
    {
        // Act
        var response1 = await _client.GetAsync("/temperature/1");
        var response2 = await _client.GetAsync("/temperature/1");

        var data1 = await response1.Content.ReadFromJsonAsync<TemperatureResponse>();
        var data2 = await response2.Content.ReadFromJsonAsync<TemperatureResponse>();

        // Assert
        Assert.NotNull(data1);
        Assert.NotNull(data2);
        // С вероятностью 99.9% значения будут разные (случайные числа)
        Assert.NotEqual(data1.value, data2.value);
    }

    private record TemperatureResponse(
        double value,
        string unit,
        DateTime timestamp,
        string location,
        string status,
        string sensor_id,
        string sensor_type,
        string description
    );
}
