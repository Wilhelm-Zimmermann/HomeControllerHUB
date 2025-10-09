using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Privileges.Queries;
using HomeControllerHUB.Application.Privileges.Queries.PrivilegeSelector;
using HomeControllerHUB.Application.Users.Queries.GetCurrentUser;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HomeControllerHUB.Application.Tests.Privileges.Queries;

public class PrivilegeSelectorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ApiUserManager> _userManagerMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PrivilegeSelectorQueryValidator _validator;

    public PrivilegeSelectorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Privilege, PrivilegeSelectorDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Description));
        });
        _mapper = config.CreateMapper();
        
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _resourceMock = new Mock<ISharedResource>();
        _mediatorMock = new Mock<IMediator>();
        _validator = new PrivilegeSelectorQueryValidator();

        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passValidators = new List<IPasswordValidator<ApplicationUser>>();
        _userManagerMock = new Mock<ApiUserManager>(store.Object, options.Object, null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object, userValidators, passValidators,
            new Mock<ILookupNormalizer>().Object, new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object, new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }

    private async Task SeedPrivileges()
    {
        var establishment = await CreateEstablishment();
        var domain = new ApplicationDomain { Name = "Test Domain" };
        _context.Domains.Add(domain);

        _context.Privilege.AddRange(
            new Privilege { Name = "locations-read", Description = "Read Locations", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" },
            new Privilege { Name = "locations-write", Description = "Write Locations", EstablishmentId = establishment.Id, Domain = domain, Actions = "Write" },
            new Privilege { Name = "sensors-read", Description = "Read Sensors", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" }
        );
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_Should_ReturnAllPrivileges_ForPlatformAdmin()
    {
        // ARRANGE
        await SeedPrivileges();
        var userDto = new CurrentUserDto { Privileges = new List<string> { "platform-all" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        var query = new PrivilegeSelectorQuery(null);
        var handler = new PrivilegeSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object, _userManagerMock.Object, _resourceMock.Object, _mediatorMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Get_Should_ReturnSpecificPrivileges_ForGroupAdmin()
    {
        // ARRANGE
        await SeedPrivileges();
        var userDto = new CurrentUserDto { Privileges = new List<string> { "locations-all" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        var query = new PrivilegeSelectorQuery(null);
        var handler = new PrivilegeSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object, _userManagerMock.Object, _resourceMock.Object, _mediatorMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "locations-read");
        result.Should().Contain(p => p.Name == "locations-write");
    }

    [Fact]
    public async Task Get_Should_FilterBySearch()
    {
        // ARRANGE
        await SeedPrivileges();
        var userDto = new CurrentUserDto { Privileges = new List<string> { "platform-all" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
        
        var query = new PrivilegeSelectorQuery("sensor");
        var handler = new PrivilegeSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object, _userManagerMock.Object, _resourceMock.Object, _mediatorMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("sensors-read");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void Validation_Should_Fail_WhenSearchByIsTooShort(string searchTerm)
    {
        // ARRANGE
        var query = new PrivilegeSelectorQuery(searchTerm);
        
        // ACT
        var result = _validator.TestValidate(query);
        
        // ASSERT
        result.ShouldHaveValidationErrorFor(q => q.SearchBy);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    public void Validation_Should_Succeed_WhenSearchByIsValid(string searchTerm)
    {
        // ARRANGE
        var query = new PrivilegeSelectorQuery(searchTerm);
        
        // ACT
        var result = _validator.TestValidate(query);
        
        // ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }
}
