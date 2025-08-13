using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentById;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Establishments.Queries;

public class GetEstablishmentByIdQueryTests : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    
    public GetEstablishmentByIdQueryTests()
    {
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile(typeof(GetEstablishmentByIdQueryHandler).Assembly));
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    
    [Fact]
    public async Task Get_Should_Return_One_EstablishmentById()
    {
        // ARRANGE
        var id = Guid.NewGuid();
        var newEstablishment = new Establishment
        {
            Id = id,
            Name = "Testes com o id",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        _context.Establishments.Add(newEstablishment);
        await _context.SaveChangesAsync();
        
        var query = new GetEstablishmentByIdQuery(id);
        var handler = new GetEstablishmentByIdQueryHandler(_context, _mapper);
        // ACT
        var result = await handler.Handle(query, CancellationToken.None);
        
        // ASSERT
        result.Id.Should().Be(id);
    }
}