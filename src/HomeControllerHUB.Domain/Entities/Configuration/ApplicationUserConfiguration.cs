using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(p => p.Code)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("varchar(50)")
            .HasDefaultValueSql("nextval('ApplicationUserCodeSequence')");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.NormalizedName)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.PasswordHash)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(p => p.Login)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(p => p.Document)
            .HasMaxLength(11);

        builder.Property(x => x.EstablishmentId)
            .IsRequired();

        builder.HasIndex(p => p.Login)
            .IsUnique();
    }
}