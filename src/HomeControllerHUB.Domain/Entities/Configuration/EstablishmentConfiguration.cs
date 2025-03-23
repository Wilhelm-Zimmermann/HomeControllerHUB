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
            .ValueGeneratedOnAdd()
            .HasColumnType("varchar(50)")
            .HasDefaultValueSql("nextval('EstablishmentCodeSequence')");

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
    }
}