using HomeControllerHUB.Domain;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Entities.Configuration;
using HomeControllerHUB.Infra.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HomeControllerHUB.Infra.DatabaseContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly NormalizedInterceptor _normalizedInterceptor;
    private readonly BaseEntityInterceptor _baseEntityInterceptor;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, NormalizedInterceptor normalizedInterceptor, BaseEntityInterceptor baseEntityInterceptor) : base(options)
    {
        _normalizedInterceptor = normalizedInterceptor;
        _baseEntityInterceptor = baseEntityInterceptor;
    }
    
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
    
    // IoT related entities
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<SensorAlert> SensorAlerts => Set<SensorAlert>();
    public DbSet<SensorStatusUpdate> SensorStatusUpdates => Set<SensorStatusUpdate>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationDomainConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationMenuConfiguration());
        modelBuilder.ApplyConfiguration(new EstablishmentConfiguration());
        modelBuilder.ApplyConfiguration(new GenericConfiguration());
        modelBuilder.ApplyConfiguration(new PrivilegeConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileConfiguration());
        
        // IoT related configurations
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new SensorConfiguration());
        modelBuilder.ApplyConfiguration(new SensorReadingConfiguration());
        modelBuilder.ApplyConfiguration(new SensorAlertConfiguration());
        modelBuilder.ApplyConfiguration(new SensorStatusUpdateConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_normalizedInterceptor);
        optionsBuilder.AddInterceptors(_baseEntityInterceptor);
    }
}