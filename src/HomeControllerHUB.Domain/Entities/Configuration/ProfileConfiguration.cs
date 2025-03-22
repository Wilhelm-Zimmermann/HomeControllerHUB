using HomeControllerHUB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasMany(p => p.ProfilePrivileges)
            .WithOne(t => t.Profile)
            .HasForeignKey(f => f.ProfileId);
        builder.HasKey(x => x.Id);
    }
}