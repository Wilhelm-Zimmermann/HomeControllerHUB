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
        
        var query = new GetAllEstablishmentPaginatedQuery();
        var handler = new GetAllEstablishmentPaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);
        // ACT
        var result = await handler.Handle(query, CancellationToken.None);
        
        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be(newEstablishment.Id);
        result.Items[0].Name.Should().Be(newEstablishment.Name);
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
}