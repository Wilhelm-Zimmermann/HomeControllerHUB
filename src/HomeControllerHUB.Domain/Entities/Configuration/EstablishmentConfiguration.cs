using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class EstablishmentConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.Property(e => e.Code)
            .IsRequired()
            .HasColumnType("varchar(50)")
            .HasDefaultValueSql("nextval('EstablishmentCodeSequence')")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(x => x.SiteName)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(x => x.Document)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(x => x.IsMaster)
            .IsRequired();

        builder.Property(x => x.Enable)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(x => x.NormalizedSiteName)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);
        
        // SubscriptionPlan relationship
        builder.Property(x => x.SubscriptionEndDate);
        
        builder.HasOne(x => x.SubscriptionPlan)
            .WithMany(x => x.Establishments)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}