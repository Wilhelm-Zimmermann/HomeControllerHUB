using HomeControllerHUB.Shared.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).HasMaxLength(Constants.LongTextSize);
        builder.Property(x => x.Action).HasMaxLength(100);
        builder.Property(x => x.EntityName).HasMaxLength(100);
        builder.Property(x => x.EntityId).HasMaxLength(100);
        builder.Property(x => x.EntityDisplayName).HasMaxLength(Constants.LongTextSize);
        builder.Property(x => x.Description).HasMaxLength(Constants.LongTextSize);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(Constants.LongTextSize);

        builder.HasIndex(x => x.Created);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EstablishmentId);
        builder.HasIndex(x => x.EntityName);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.Action);
    }
}
