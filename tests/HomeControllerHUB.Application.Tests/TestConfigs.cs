using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace HomeControllerHUB.Application.Tests;

public class TestConfigs : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();
    
    
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private NormalizedInterceptor _normalizedInterceptor;
    private BaseEntityInterceptor _baseEntityInterceptor;
    
    protected ApplicationDbContext _context { get; private set; }

    public async Task InitializeAsync()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _normalizedInterceptor = new NormalizedInterceptor();
        _baseEntityInterceptor = new BaseEntityInterceptor(_currentUserServiceMock.Object);
        
        await _dbContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString(), sql => sql.MigrationsAssembly(typeof(HomeControllerHUB.Api.ConfigureServices).Assembly.GetName().Name))
            .Options;

        _context = new ApplicationDbContext(options, _normalizedInterceptor, _baseEntityInterceptor);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public async Task<Establishment> CreateEstablishment(string? name = "Estabelecimento teste")
    {
        var newEstablishment = new Establishment
        {
            Id = Guid.NewGuid(),
            Name = name,
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        _context.Establishments.Add(newEstablishment);
        await _context.SaveChangesAsync();
        
        return newEstablishment;
    }
}