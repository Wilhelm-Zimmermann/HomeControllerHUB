using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SensorStatusUpdateConfiguration : IEntityTypeConfiguration<SensorStatusUpdate>
{
    public void Configure(EntityTypeBuilder<SensorStatusUpdate> builder)
    {
        builder.ToTable("SensorStatusUpdates");
        
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Sensor)
            .WithMany()
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(x => x.Status)
            .HasMaxLength(100);
            
        builder.Property(x => x.SignalStrength)
            .HasMaxLength(20);
            
        // Metadata is stored as JSON
        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");
    }
} 