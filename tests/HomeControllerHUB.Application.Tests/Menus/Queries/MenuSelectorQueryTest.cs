using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Domains.Queries;
using HomeControllerHUB.Application.Menus.Queries;
using HomeControllerHUB.Application.Menus.Queries.MenuSelector;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Menus.Queries;

public class MenuSelectorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ApiUserManager> _userManagerMock;
    private readonly Mock<ISharedResource> _resourceMock;

    public MenuSelectorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ApplicationMenu, MenuDto>();
            cfg.CreateMap<ApplicationMenu, MenuParentDto>();
            cfg.CreateMap<ApplicationDomain, ApplicationDomainDto>();
        });
        _mapper = config.CreateMapper();

        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _resourceMock = new Mock<ISharedResource>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passValidators = new List<IPasswordValidator<ApplicationUser>>();

        _userManagerMock = new Mock<ApiUserManager>(
            store.Object,
            options.Object,
            null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            userValidators,
            passValidators,
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object
        );
    }

    private async Task SeedDataForNonAdmin()
    {
        var establishment = await CreateEstablishment();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Name = "Test User",
            Login = "testuser",
            EstablishmentId = establishment.Id,
            Email = "testuser@example.com",
            NormalizedName = "TEST USER",
            PasswordHash = "hashedpassword"
        };

        var domainA = new ApplicationDomain { Id = Guid.NewGuid(), Name = "Locations" };
        var domainB = new ApplicationDomain { Id = Guid.NewGuid(), Name = "Sensors" };

        var privilegeA = new Privilege
        {
            Name = "Location Access",
            DomainId = domainA.Id,
            EstablishmentId = establishment.Id,
            Description = "Access to locations.",
            Actions = "Read,Create"
        };
        var profile = new Profile { Name = "Standard Profile", EstablishmentId = establishment.Id };

        var profilePrivilege = new ProfilePrivilege { Profile = profile, Privilege = privilegeA };
        var userProfile = new UserProfile { User = user, Profile = profile };

        var menuParentA = new ApplicationMenu
        {
            Id = Guid.NewGuid(), Name = "Registrations", NormalizedName = "REGISTRATIONS", Order = 1,
            Description = "Registrations"
        };
        var menuChildA = new ApplicationMenu
        {
            Id = Guid.NewGuid(), Name = "Locations", NormalizedName = "LOCATIONS", ParentId = menuParentA.Id,
            DomainId = domainA.Id, Order = 2, Description = "Locations Menu"
        };
        var menuParentB = new ApplicationMenu
            { Id = Guid.NewGuid(), Name = "Devices", NormalizedName = "DEVICES", Order = 3, Description = "Devices" };
        var menuChildB = new ApplicationMenu
        {
            Id = Guid.NewGuid(), Name = "Sensors", NormalizedName = "SENSORS", ParentId = menuParentB.Id,
            DomainId = domainB.Id, Order = 4, Description = "Sensors Menu"
        };
        var menuHome = new ApplicationMenu
            { Id = Guid.NewGuid(), Name = "Home", NormalizedName = "HOME", Order = 0, Description = "Início" };

        _context.Domains.AddRange(domainA, domainB);
        _context.Privilege.Add(privilegeA);
        _context.Profiles.Add(profile);
        _context.Users.Add(user);
        _context.UserProfiles.Add(userProfile);
        _context.ProfilePrivileges.Add(profilePrivilege);
        _context.Menus.AddRange(menuParentA, menuChildA, menuParentB, menuChildB, menuHome);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);
    }

    [Fact]
    public async Task Get_Should_ReturnFilteredMenus_ForNonAdminUser()
    {
        // ARRANGE
        await SeedDataForNonAdmin();
        var query = new MenuSelectorQuery(null);
        var handler = new MenuSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object,
            _userManagerMock.Object, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(3);
        result.Should().Contain(m => m.Name == "Home");
        result.Should().Contain(m => m.Name == "Registrations");
        result.Should().Contain(m => m.Name == "Locations");
        result.Should().NotContain(m => m.Name == "Devices");
        result.Should().NotContain(m => m.Name == "Sensors");
    }

    [Fact]
    public async Task Get_Should_ReturnAllMenus_ForAdminUser()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Name = "Admin User",
            Login = "admin",
            EstablishmentId = establishment.Id,
            Email = "admin@example.com",
            NormalizedName = "ADMIN USER",
            PasswordHash = "hashedpassword"
        };

        var adminDomain = new ApplicationDomain { Id = Guid.NewGuid(), Name = "Admin Domain" };
        var adminPrivilege = new Privilege
        {
            Name = "platform-all",
            EstablishmentId = establishment.Id,
            DomainId = adminDomain.Id,
            Description = "Full platform access.",
            Actions = "All"
        };
        var profile = new Profile { Name = "Admin Profile", EstablishmentId = establishment.Id };

        var profilePrivilege = new ProfilePrivilege { Profile = profile, Privilege = adminPrivilege };
        var userProfile = new UserProfile { User = user, Profile = profile };

        _context.Domains.Add(adminDomain);
        _context.Privilege.Add(adminPrivilege);
        _context.Profiles.Add(profile);
        _context.Users.Add(user);
        _context.UserProfiles.Add(userProfile);
        _context.ProfilePrivileges.Add(profilePrivilege);

        _context.Menus.AddRange(
            new ApplicationMenu
            {
                Name = "Menu 1", Order = 1, NormalizedName = "MENU 1", Description = "Menu 1 Description", Enable = true
            },
            new ApplicationMenu
            {
                Name = "Menu 2", Order = 2, NormalizedName = "MENU 2", Description = "Menu 2 Description", Enable = true
            }
        );
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);

        var query = new MenuSelectorQuery(null);
        var handler = new MenuSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object,
            _userManagerMock.Object, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_Should_FilterBySearchBy()
    {
        // ARRANGE
        await SeedDataForNonAdmin();
        var query = new MenuSelectorQuery("Loca");
        var handler = new MenuSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object,
            _userManagerMock.Object, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(1);
        result.Should().Contain(m => m.Name == "Locations");
    }

    [Fact]
    public async Task Get_Should_ThrowAppError_WhenUserNotFound()
    {
        // ARRANGE
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        var query = new MenuSelectorQuery(null);
        var handler = new MenuSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object,
            _userManagerMock.Object, _resourceMock.Object);

        // ACT
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 404);
    }
}