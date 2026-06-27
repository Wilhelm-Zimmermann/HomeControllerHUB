using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Users.Commands.CreateUser;
using HomeControllerHUB.Application.Users.Commands.DeleteUser;
using HomeControllerHUB.Application.Users.Commands.UpdateUser;
using HomeControllerHUB.Application.Users.Queries.GetUserById;
using HomeControllerHUB.Application.Users.Queries.GetUserList;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using DomainProfile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Users;

public class UserManagementContractTests : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ISharedResource> _resourceMock = new();
    private readonly Mock<HomeControllerHUB.Domain.Interfaces.IEmailService> _emailServiceMock = new();

    public UserManagementContractTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile(typeof(GetUserListQueryHandler).Assembly));
        });

        _mapper = mapperConfig.CreateMapper();
        _resourceMock.Setup(r => r.NotFoundMessage(It.IsAny<string>())).Returns("Not found");
    }

    private ApiUserManager CreateUserManager()
    {
        var store = new UserStore<ApplicationUser, IdentityRole<Guid>, ApplicationDbContext, Guid>(_context);
        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions { Lockout = { AllowedForNewUsers = false } });

        return new ApiUserManager(
            store,
            options.Object,
            _context,
            new PasswordHasher<ApplicationUser>(),
            new List<IUserValidator<ApplicationUser>>(),
            new List<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }

    private async Task<DomainProfile> CreateProfile(Establishment establishment, string name)
    {
        var profile = new DomainProfile
        {
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Name = name,
            Description = $"{name} description",
            Enable = true
        };

        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        return profile;
    }

    private async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        Establishment establishment,
        string login,
        bool enable = true)
    {
        var user = new ApplicationUser
        {
            UserName = login,
            Login = login,
            Email = $"{login}@example.com",
            Name = login,
            Document = "12345678901",
            EmailConfirmed = true,
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Enable = enable
        };

        var result = await userManager.CreateAsync(user, "Password123!");
        result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(error => error.Description)));

        return user;
    }

    private async Task AddLinks(ApplicationUser user, IEnumerable<DomainProfile> profiles, IEnumerable<Establishment> establishments)
    {
        foreach (var profile in profiles)
        {
            _context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                ProfileId = profile.Id
            });
        }

        foreach (var establishment in establishments)
        {
            _context.UserEstablishments.Add(new UserEstablishment
            {
                UserId = user.Id,
                EstablishmentId = establishment.Id
            });
        }

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserList_Should_Return_Paginated_Filtered_Users()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var otherEstablishment = await CreateEstablishment("Other");
        await CreateUserAsync(userManager, establishment, "alpha");
        await CreateUserAsync(userManager, establishment, "beta", enable: false);
        await CreateUserAsync(userManager, otherEstablishment, "gamma");

        var handler = new GetUserListQueryHandler(_context, _mapper);

        var page = await handler.Handle(new GetUserListQuery
        {
            PageNumber = 1,
            PageSize = 2
        }, CancellationToken.None);

        var filtered = await handler.Handle(new GetUserListQuery
        {
            SearchBy = "alp",
            Enable = true,
            EstablishmentId = establishment.Id
        }, CancellationToken.None);

        page.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(3);
        page.HasNextPage.Should().BeTrue();
        filtered.Items.Should().ContainSingle();
        filtered.Items[0].Login.Should().Be("alpha");
        filtered.Items[0].EstablishmentId.Should().Be(establishment.Id);
    }

    [Fact]
    public async Task GetUserById_Should_Return_Profile_And_Establishment_Ids()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var linkedEstablishment = await CreateEstablishment("Linked");
        var profile = await CreateProfile(establishment, "Admin");
        var secondProfile = await CreateProfile(establishment, "Operator");
        var user = await CreateUserAsync(userManager, establishment, "detail");
        await AddLinks(user, [profile, secondProfile], [establishment, linkedEstablishment]);

        var handler = new GetUserByIdQueryHandler(_context, _resourceMock.Object);

        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        result.EstablishmentId.Should().Be(establishment.Id);
        result.EstablishmentName.Should().Be("Main");
        result.ProfileIds.Should().BeEquivalentTo([profile.Id, secondProfile.Id]);
        result.EstablishmentIds.Should().BeEquivalentTo([establishment.Id, linkedEstablishment.Id]);
        result.Profiles.Select(p => p.Name).Should().BeEquivalentTo(["Admin", "Operator"]);
        result.Establishments.Select(e => e.Name).Should().BeEquivalentTo(["Main", "Linked"]);
    }

    [Fact]
    public async Task CreateUser_Should_Create_User_With_Profile_And_Establishment_Aliases()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var profile = await CreateProfile(establishment, "Admin");
        var handler = new CreateUserCommandHandler(userManager, _context, _emailServiceMock.Object);

        var result = await handler.Handle(new CreateUserCommand
        {
            Login = "created",
            Email = "created@example.com",
            Password = "Password123!",
            Name = "Created User",
            Document = "12345678901",
            EstablishmentId = establishment.Id,
            Enable = false,
            ProfileIds = [profile.Id],
            EstablishmentIds = [establishment.Id]
        }, CancellationToken.None);

        var user = await _context.Users.FindAsync(result.Id);
        user.Should().NotBeNull();
        user!.Enable.Should().BeFalse();
        _context.UserProfiles.Where(up => up.UserId == result.Id).Select(up => up.ProfileId).Should().Equal(profile.Id);
        _context.UserEstablishments.Where(ue => ue.UserId == result.Id).Select(ue => ue.EstablishmentId).Should().Equal(establishment.Id);
        _emailServiceMock.Verify(s => s.SendEmailConfirmationAsync("created@example.com", "Created User", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_Should_Preserve_Links_When_Alias_Lists_Are_Null()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var linkedEstablishment = await CreateEstablishment("Linked");
        var profile = await CreateProfile(establishment, "Admin");
        var user = await CreateUserAsync(userManager, establishment, "preserve");
        await AddLinks(user, [profile], [linkedEstablishment]);
        var originalPasswordHash = user.PasswordHash;
        var handler = new UpdateUserCommandHandler(userManager, _context, _resourceMock.Object);

        await handler.Handle(new UpdateUserCommand
        {
            Id = user.Id,
            Name = "Preserved",
            Login = "preserve-updated",
            Email = "preserved@example.com",
            Document = "12345678901",
            EstablishmentId = establishment.Id,
            Enable = true
        }, CancellationToken.None);

        var updatedUser = await userManager.FindByIdAsync(user.Id.ToString());
        updatedUser!.PasswordHash.Should().Be(originalPasswordHash);
        _context.UserProfiles.Where(up => up.UserId == user.Id).Select(up => up.ProfileId).Should().Equal(profile.Id);
        _context.UserEstablishments.Where(ue => ue.UserId == user.Id).Select(ue => ue.EstablishmentId).Should().Equal(linkedEstablishment.Id);
    }

    [Fact]
    public async Task UpdateUser_Should_Replace_Links_When_Alias_Lists_Are_Provided()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var oldEstablishment = await CreateEstablishment("Old");
        var newEstablishment = await CreateEstablishment("New");
        var oldProfile = await CreateProfile(establishment, "OldProfile");
        var newProfile = await CreateProfile(establishment, "NewProfile");
        var user = await CreateUserAsync(userManager, establishment, "replace");
        await AddLinks(user, [oldProfile], [oldEstablishment]);
        var handler = new UpdateUserCommandHandler(userManager, _context, _resourceMock.Object);

        await handler.Handle(new UpdateUserCommand
        {
            Id = user.Id,
            Name = "Replace",
            Login = "replace-updated",
            Email = "replace@example.com",
            Document = "12345678901",
            EstablishmentId = establishment.Id,
            Enable = true,
            ProfileIds = [newProfile.Id],
            EstablishmentIds = [newEstablishment.Id]
        }, CancellationToken.None);

        _context.UserProfiles.Where(up => up.UserId == user.Id).Select(up => up.ProfileId).Should().Equal(newProfile.Id);
        _context.UserEstablishments.Where(ue => ue.UserId == user.Id).Select(ue => ue.EstablishmentId).Should().Equal(newEstablishment.Id);
    }

    [Fact]
    public async Task DeleteUser_Should_Delete_User_And_Related_Links()
    {
        var userManager = CreateUserManager();
        var establishment = await CreateEstablishment("Main");
        var linkedEstablishment = await CreateEstablishment("Linked");
        var profile = await CreateProfile(establishment, "Admin");
        var user = await CreateUserAsync(userManager, establishment, "delete");
        await AddLinks(user, [profile], [linkedEstablishment]);
        var handler = new DeleteUserCommandHandler(userManager, _context, _resourceMock.Object);

        await handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        (await userManager.FindByIdAsync(user.Id.ToString())).Should().BeNull();
        _context.UserProfiles.Where(up => up.UserId == user.Id).Should().BeEmpty();
        _context.UserEstablishments.Where(ue => ue.UserId == user.Id).Should().BeEmpty();
    }
}
