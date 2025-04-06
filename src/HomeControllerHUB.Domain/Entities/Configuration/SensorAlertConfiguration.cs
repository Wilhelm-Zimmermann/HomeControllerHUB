using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SensorAlertConfiguration : IEntityTypeConfiguration<SensorAlert>
{
    public void Configure(EntityTypeBuilder<SensorAlert> builder)
    {
        builder.ToTable("SensorAlerts");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.IsAcknowledged).IsRequired().HasDefaultValue(false);
        
        builder.HasIndex(x => x.SensorId);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.IsAcknowledged);
        
        builder.HasOne(x => x.Sensor)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.AcknowledgedBy)
            .WithMany()
            .HasForeignKey(x => x.AcknowledgedById)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
} 