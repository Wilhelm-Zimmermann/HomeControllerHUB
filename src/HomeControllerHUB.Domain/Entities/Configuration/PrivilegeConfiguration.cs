using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(p => p.NormalizedName)
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(p => p.NormalizedDescription)
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.Enable)
            .IsRequired();

        builder.Property(x => x.Actions)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);
    }
}