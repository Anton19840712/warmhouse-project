using Microsoft.EntityFrameworkCore;
using TelemetryService.Models;

namespace TelemetryService.Data;

public class TelemetryContext : DbContext
{
    public TelemetryContext(DbContextOptions<TelemetryContext> options) : base(options)
    {
    }

    public DbSet<TelemetryReading> TelemetryReadings { get; set; }
    public DbSet<Sensor> Sensors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired();
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.SensorType).HasMaxLength(50);

            entity.HasIndex(e => e.SensorId);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(20);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(e => e.LastUpdated).HasColumnName("last_updated").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        });
    }
}
