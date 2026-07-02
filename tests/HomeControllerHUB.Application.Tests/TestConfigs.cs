using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeControllerHUB.Application.Tests;

public class TestConfigs : IAsyncLifetime
{
    private static readonly SemaphoreSlim ContainerLock = new(1, 1);
    private static PostgreSqlContainer? _dbContainer;

    private string? _databaseName;
    
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private NormalizedInterceptor _normalizedInterceptor;
    private BaseEntityInterceptor _baseEntityInterceptor;
    
    protected ApplicationDbContext _context { get; private set; }

    public async Task InitializeAsync()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _normalizedInterceptor = new NormalizedInterceptor();
        _baseEntityInterceptor = new BaseEntityInterceptor(_currentUserServiceMock.Object);

        var container = await GetContainerAsync();
        _databaseName = $"test_{Guid.NewGuid():N}";
        var connectionString = await CreateDatabaseAsync(container, _databaseName);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(HomeControllerHUB.Api.ConfigureServices).Assembly.GetName().Name))
            .Options;

        _context = new ApplicationDbContext(options, _normalizedInterceptor, _baseEntityInterceptor);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();

        if (_databaseName is not null && _dbContainer is not null)
        {
            await DropDatabaseAsync(_dbContainer, _databaseName);
        }
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

    private static async Task<PostgreSqlContainer> GetContainerAsync()
    {
        if (_dbContainer is not null)
        {
            return _dbContainer;
        }

        await ContainerLock.WaitAsync();
        try
        {
            if (_dbContainer is not null)
            {
                return _dbContainer;
            }

            _dbContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .Build();

            await _dbContainer.StartAsync();
            return _dbContainer;
        }
        finally
        {
            ContainerLock.Release();
        }
    }

    private static async Task<string> CreateDatabaseAsync(PostgreSqlContainer container, string databaseName)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(container.GetConnectionString());
        var adminConnectionString = connectionStringBuilder.ConnectionString;

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)}";
        await command.ExecuteNonQueryAsync();

        connectionStringBuilder.Database = databaseName;
        return connectionStringBuilder.ConnectionString;
    }

    private static async Task DropDatabaseAsync(PostgreSqlContainer container, string databaseName)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(container.GetConnectionString());

        await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync();

        await using var terminateCommand = connection.CreateCommand();
        terminateCommand.CommandText = """
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = @databaseName AND pid <> pg_backend_pid()
            """;
        terminateCommand.Parameters.AddWithValue("databaseName", databaseName);
        await terminateCommand.ExecuteNonQueryAsync();

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(databaseName)}";
        await dropCommand.ExecuteNonQueryAsync();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
