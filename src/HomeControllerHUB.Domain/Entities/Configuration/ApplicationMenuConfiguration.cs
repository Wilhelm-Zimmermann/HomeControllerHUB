using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class ApplicationMenuConfiguration : IEntityTypeConfiguration<ApplicationMenu>
{
    public void Configure(EntityTypeBuilder<ApplicationMenu> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(p => p.NormalizedName);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.NormalizedDescription);

        builder.Property(c => c.IconClass)
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(c => c.Link)
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(c => c.Target)
            .HasMaxLength(Constants.TinyTextSize);

        builder.HasIndex(p => p.NormalizedName);
        builder.HasIndex(p => p.NormalizedDescription);
    }
}