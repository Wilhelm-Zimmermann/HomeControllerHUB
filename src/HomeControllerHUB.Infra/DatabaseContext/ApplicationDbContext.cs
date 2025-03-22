using HomeControllerHUB.Domain;
using HomeControllerHUB.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Infra.DatabaseContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<ApplicationDomain> Domains => Set<ApplicationDomain>();
    public DbSet<ApplicationMenu> Menus => Set<ApplicationMenu>();
    public DbSet<Establishment> Establishments => Set<Establishment>();
    public DbSet<Generic> Generics => Set<Generic>();
    public DbSet<Privilege> Privilege => Set<Privilege>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfilePrivilege> ProfilePrivileges => Set<ProfilePrivilege>();
    public DbSet<UserEstablishment> UserEstablishments => Set<UserEstablishment>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigureServices).Assembly);
    }
}