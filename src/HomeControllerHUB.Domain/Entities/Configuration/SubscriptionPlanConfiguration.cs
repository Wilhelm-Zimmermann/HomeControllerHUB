using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.NormalizedName).HasMaxLength(255);
        builder.Property(x => x.NormalizedDescription).HasMaxLength(500);
        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.MaxSensors).IsRequired();
        builder.Property(x => x.DataRetentionDays).IsRequired();
        builder.Property(x => x.AlertsPerMonth).IsRequired();
        builder.Property(x => x.IncludesReporting).IsRequired();
        builder.Property(x => x.IncludesApiAccess).IsRequired();
        
        builder.HasIndex(x => x.NormalizedName).IsUnique();
    }
} 