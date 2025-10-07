using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Generics.Queries;
using HomeControllerHUB.Application.Generics.Queries.GenericSelector;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Generics.Queries;

public class GenericSelectorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ISharedResource> _resourceMock;
    
    public GenericSelectorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Generic, GenericDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Value));
        });
        _mapper = config.CreateMapper();

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _resourceMock = new Mock<ISharedResource>();
    }
    
    [Fact]
    public async Task Get_Should_ReturnFilteredListByIdentifier()
    {
        // ARRANGE
        const string targetIdentifier = "DEVICE_TYPES";
        _context.Generics.AddRange(
            new Generic { Identifier = targetIdentifier, Code = "ARDUINO", Value = "Arduino", DisplayOrder = 1 },
            new Generic { Identifier = targetIdentifier, Code = "ESP32", Value = "ESP32", DisplayOrder = 2  },
            new Generic { Identifier = "OTHER_TYPES", Code = "SENSOR", Value = "Sensor", DisplayOrder = 3  }
        );
        await _context.SaveChangesAsync();
        
        var query = new GenericSelectorQuery(targetIdentifier);
        var handler = new GenericSelectorQueryHandler(_currentUserServiceMock.Object, _context, _mapper, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(dto => dto.Identifier == targetIdentifier);
        result.First(dto => dto.Code == "ARDUINO").Name.Should().Be("Arduino");
    }
    
    [Fact]
    public async Task Get_Should_ReturnEmptyList_WhenIdentifierDoesNotExist()
    {
        // ARRANGE
        _context.Generics.AddRange(
            new Generic { Identifier = "DEVICE_TYPES", Code = "ARDUINO", Value = "Arduino", DisplayOrder = 1  }
        );
        await _context.SaveChangesAsync();
        
        var query = new GenericSelectorQuery("NON_EXISTENT_IDENTIFIER");
        var handler = new GenericSelectorQueryHandler(_currentUserServiceMock.Object, _context, _mapper, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
