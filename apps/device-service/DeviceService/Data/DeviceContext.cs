using Microsoft.EntityFrameworkCore;
using DeviceService.Models;

namespace DeviceService.Data;

public class DeviceContext : DbContext
{
    public DeviceContext(DbContextOptions<DeviceContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceCommand> DeviceCommands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SerialNumber).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<DeviceCommand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).IsRequired();
            entity.Property(e => e.Command).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.Status);
        });
    }
}
