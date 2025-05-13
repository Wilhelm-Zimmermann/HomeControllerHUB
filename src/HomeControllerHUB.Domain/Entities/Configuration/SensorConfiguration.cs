using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("Sensors");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FirmwareVersion).HasMaxLength(50);
        builder.Property(x => x.ApiKey).HasMaxLength(100);
        builder.Property(x => x.NormalizedName).HasMaxLength(255);
        
        builder.HasIndex(x => x.DeviceId).IsUnique();
        builder.HasIndex(x => x.NormalizedName);
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.LocationId);
        
        builder.HasOne(x => x.Establishment)
            .WithMany(x => x.Sensors)
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.Location)
            .WithMany(x => x.Sensors)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 