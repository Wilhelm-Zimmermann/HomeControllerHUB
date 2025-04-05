using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
{
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        builder.ToTable("SensorReadings");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(20);
        builder.Property(x => x.RawData).HasMaxLength(1000);
        
        // Metadata is stored as JSON
        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");
        
        builder.HasIndex(x => x.SensorId);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.SensorId, x.Timestamp });
        
        builder.HasOne(x => x.Sensor)
            .WithMany(x => x.Readings)
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 