using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class ApplicationDomainConfiguration : IEntityTypeConfiguration<ApplicationDomain>
{
    public void Configure(EntityTypeBuilder<ApplicationDomain> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.NormalizedName);

        builder.Property(p => p.Description)
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.NormalizedDescription);

        builder.Property(p => p.Enable)
            .IsRequired();

        builder.HasIndex(p => p.NormalizedName);
        builder.HasIndex(p => p.NormalizedDescription);
    }
}