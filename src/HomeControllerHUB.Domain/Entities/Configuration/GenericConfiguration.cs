using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class GenericConfiguration : IEntityTypeConfiguration<Generic>
{
    public void Configure(EntityTypeBuilder<Generic> builder)
    {
        builder.Property(x => x.Identifier)
            .IsRequired()
            .HasMaxLength(Constants.MediumTextSize);

        builder.Property(x => x.Code)
            .IsRequired()
            .ValueGeneratedOnAdd()
            .HasColumnType("varchar(50)")
            .HasDefaultValueSql("nextval('GenericCodeSequence')");

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(x => x.NormalizedValue)
            .IsRequired()
            .HasMaxLength(Constants.LongTextSize);

        builder.Property(x => x.Enable)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();
    }
}