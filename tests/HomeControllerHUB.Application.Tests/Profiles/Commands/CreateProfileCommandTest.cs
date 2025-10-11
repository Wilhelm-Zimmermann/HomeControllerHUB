using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Profiles.Commands.CreateProfile;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Profiles.Commands;

public class CreateProfileCommandTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateProfileCommandValidator _validator;

    public CreateProfileCommandTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _validator = new CreateProfileCommandValidator();
    }

    private async Task<List<Privilege>> SeedPrivileges()
    {
        var establishment = await CreateEstablishment();
        var domain = new ApplicationDomain { Name = "Test Domain" };
        _context.Domains.Add(domain);

        var privileges = new List<Privilege>
        {
            new() { Name = "priv-1", Description = "Privilege 1", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" },
            new() { Name = "priv-2", Description = "Privilege 2", EstablishmentId = establishment.Id, Domain = domain, Actions = "Write" }
        };
        _context.Privilege.AddRange(privileges);
        await _context.SaveChangesAsync();
        return privileges;
    }

    [Fact]
    public async Task Create_Should_Succeed_WithValidParameters()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);

        var privileges = await SeedPrivileges();
        var command = new CreateProfileCommand
        {
            Name = "Admin Profile",
            Description = "Profile for administrators",
            Enable = true,
            PrivilegeIds = privileges.Select(p => p.Id).ToList()
        };
        
        var handler = new CreateProfileCommandHandler(_context, _mapperMock.Object, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        result.Id.Should().NotBe(Guid.Empty);
        var profileInDb = await _context.Profiles.FindAsync(result.Id);
        profileInDb.Should().NotBeNull();
        profileInDb!.Name.Should().Be(command.Name);
        profileInDb.EstablishmentId.Should().Be(establishment.Id);

        var profilePrivilegesCount = _context.ProfilePrivileges.Count(pp => pp.ProfileId == result.Id);
        profilePrivilegesCount.Should().Be(2);
    }

    [Fact]
    public async Task Create_Should_ThrowAppError_WhenPrivilegeNotFound()
    {
        // ARRANGE
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(Guid.NewGuid());
        var command = new CreateProfileCommand
        {
            Name = "Test Profile",
            PrivilegeIds = new List<Guid> { Guid.NewGuid() }
        };
        var handler = new CreateProfileCommandHandler(_context, _mapperMock.Object, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 404);
    }

    [Fact]
    public void Validation_Should_Fail_WhenNameIsEmpty()
    {
        // ARRANGE
        var command = new CreateProfileCommand { Name = "" };

        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
