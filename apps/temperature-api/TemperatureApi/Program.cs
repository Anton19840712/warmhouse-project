using Serilog;
using TemperatureApi;

namespace TemperatureApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, configuration) =>
            configuration.WriteTo.Console());

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "temperature-api" }))
            .WithName("HealthCheck");

        app.MapTemperatureEndpoints();

        app.Run();
    }
}
