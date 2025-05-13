using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeControllerHUB.Domain.Entities.Configuration;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.NormalizedName).HasMaxLength(255);
        builder.Property(x => x.NormalizedDescription).HasMaxLength(500);
        
        builder.HasIndex(x => x.NormalizedName);
        builder.HasIndex(x => x.EstablishmentId);
        
        builder.HasOne(x => x.Establishment)
            .WithMany(x => x.Locations)
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(x => x.ParentLocation)
            .WithMany(x => x.ChildLocations)
            .HasForeignKey(x => x.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
} 