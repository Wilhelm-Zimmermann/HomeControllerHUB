using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Establishments.Queries.GetAllEstablishmentPaginated;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Establishments.Queries;

public class GetAllEstablishmentPaginatedQueryTests : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetAllEstablishmentPaginatedQueryTests()
    {
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile(typeof(GetAllEstablishmentPaginatedQueryHandler).Assembly));
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    [Fact]
    public async Task Get_Should_Return_All_Establishments()
    {
        // ARRANGE
        var newEstablishment = await CreateEstablishment();
        var inactiveEstablishment = await CreateEstablishment("Inactive");
        inactiveEstablishment.Enable = false;
        await _context.SaveChangesAsync();
        
        var query = new GetAllEstablishmentPaginatedQuery();
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);
        // ACT
        var result = await handler.Handle(query, CancellationToken.None);
        
        // ASSERT
        result.Items.Should().HaveCount(2);
        result.Items.Select(establishment => establishment.Id)
            .Should()
            .Contain(new[] { newEstablishment.Id, inactiveEstablishment.Id });
    }
    
    
    [Fact]
    public async Task Get_Should_Return_All_Establishments_MatchingSearch()
    {
        // ARRANGE
        await CreateEstablishment();
        var novoEstab = await CreateEstablishment("NovoNome");
        
        var query = new GetAllEstablishmentPaginatedQuery()
        {
            SearchBy = "Novo"
        };
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);
        // ACT
        var result = await handler.Handle(query, CancellationToken.None);
        
        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be(novoEstab.Name);
    }

    [Fact]
    public async Task Get_Should_Return_Only_Enabled_Establishments_WhenEnableIsTrue()
    {
        // ARRANGE
        var activeEstablishment = await CreateEstablishment("Active");
        var inactiveEstablishment = await CreateEstablishment("Inactive");
        inactiveEstablishment.Enable = false;
        await _context.SaveChangesAsync();

        var query = new GetAllEstablishmentPaginatedQuery
        {
            Enable = true
        };
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(activeEstablishment.Id);
    }

    [Fact]
    public async Task Get_Should_Return_Only_Disabled_Establishments_WhenEnableIsFalse()
    {
        // ARRANGE
        await CreateEstablishment("Active");
        var inactiveEstablishment = await CreateEstablishment("Inactive");
        inactiveEstablishment.Enable = false;
        await _context.SaveChangesAsync();

        var query = new GetAllEstablishmentPaginatedQuery
        {
            Enable = false
        };
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(inactiveEstablishment.Id);
    }

    [Fact]
    public async Task Get_Should_Keep_Pagination_WhenFilteringByEnable()
    {
        // ARRANGE
        await CreateEstablishment("Active 1");
        await CreateEstablishment("Active 2");
        var inactiveEstablishment = await CreateEstablishment("Inactive");
        inactiveEstablishment.Enable = false;
        await _context.SaveChangesAsync();

        var query = new GetAllEstablishmentPaginatedQuery
        {
            Enable = true,
            PageNumber = 1,
            PageSize = 1
        };
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Should_Apply_Search_And_Enable_Filter_Together()
    {
        // ARRANGE
        await CreateEstablishment("Target Active");
        var inactiveEstablishment = await CreateEstablishment("Target Inactive");
        inactiveEstablishment.Enable = false;
        await CreateEstablishment("Other Active");
        await _context.SaveChangesAsync();

        var query = new GetAllEstablishmentPaginatedQuery
        {
            Enable = true,
            SearchBy = "Target"
        };
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Target Active");
    }
}
