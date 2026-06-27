using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Profiles.Commands.UpdateProfile;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HomeControllerHUB.Application.Tests.Profiles.Commands;

public class UpdateProfileCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateProfileCommandValidator _validator;

    public UpdateProfileCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _validator = new UpdateProfileCommandValidator();
    }
    
    [Fact]
    public async Task Update_Should_Succeed_AndReplacePrivileges()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);
        var domain = new ApplicationDomain { Name = "Test Domain" };
        _context.Domains.Add(domain);

        var initialPrivilege = new Privilege { Name = "old-priv", Description = "Old", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" };
        var newPrivilege = new Privilege { Name = "new-priv", Description = "New", EstablishmentId = establishment.Id, Domain = domain, Actions = "Write" };
        _context.Privilege.AddRange(initialPrivilege, newPrivilege);

        var profile = new Profile { Name = "Original Name", EstablishmentId = establishment.Id };
        _context.Profiles.Add(profile);
        _context.ProfilePrivileges.Add(new ProfilePrivilege { Profile = profile, Privilege = initialPrivilege });
        await _context.SaveChangesAsync();

        var command = new UpdateProfileCommand
        {
            Id = profile.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Enable = false,
            PrivilegeIds = new List<Guid> { newPrivilege.Id }
        };
        var handler = new UpdateProfilesCommandHandler(_context, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var profileInDb = await _context.Profiles.FindAsync(profile.Id);
        profileInDb!.Name.Should().Be(command.Name);
        profileInDb.Enable.Should().Be(command.Enable);

        var privilegesInDb = await _context.ProfilePrivileges
            .Where(pp => pp.ProfileId == profile.Id)
            .ToListAsync();
        
        privilegesInDb.Should().HaveCount(1);
        privilegesInDb.First().PrivilegeId.Should().Be(newPrivilege.Id);
    }

    [Fact]
    public async Task Update_Should_PreservePrivileges_WhenPrivilegeIdsIsNull()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);
        var domain = new ApplicationDomain { Name = "Test Domain" };
        _context.Domains.Add(domain);

        var initialPrivilege = new Privilege { Name = "old-priv", Description = "Old", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" };
        var profile = new Profile { Name = "Original Name", EstablishmentId = establishment.Id };
        _context.Privilege.Add(initialPrivilege);
        _context.Profiles.Add(profile);
        _context.ProfilePrivileges.Add(new ProfilePrivilege { Profile = profile, Privilege = initialPrivilege });
        await _context.SaveChangesAsync();

        var command = new UpdateProfileCommand
        {
            Id = profile.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Enable = true,
            PrivilegeIds = null
        };
        var handler = new UpdateProfilesCommandHandler(_context, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var privilegesInDb = await _context.ProfilePrivileges
            .Where(pp => pp.ProfileId == profile.Id)
            .ToListAsync();

        privilegesInDb.Should().ContainSingle();
        privilegesInDb.First().PrivilegeId.Should().Be(initialPrivilege.Id);
    }

    [Fact]
    public async Task Update_Should_RemoveAllPrivileges_WhenPrivilegeIdsIsEmpty()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);
        var domain = new ApplicationDomain { Name = "Test Domain" };
        _context.Domains.Add(domain);

        var initialPrivilege = new Privilege { Name = "old-priv", Description = "Old", EstablishmentId = establishment.Id, Domain = domain, Actions = "Read" };
        var profile = new Profile { Name = "Original Name", EstablishmentId = establishment.Id };
        _context.Privilege.Add(initialPrivilege);
        _context.Profiles.Add(profile);
        _context.ProfilePrivileges.Add(new ProfilePrivilege { Profile = profile, Privilege = initialPrivilege });
        await _context.SaveChangesAsync();

        var command = new UpdateProfileCommand
        {
            Id = profile.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Enable = true,
            PrivilegeIds = new List<Guid>()
        };
        var handler = new UpdateProfilesCommandHandler(_context, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var privilegesInDb = await _context.ProfilePrivileges
            .Where(pp => pp.ProfileId == profile.Id)
            .ToListAsync();

        privilegesInDb.Should().BeEmpty();
    }

    [Fact]
    public void Validation_Should_Succeed_WhenPrivilegeIdsAreEmpty()
    {
        // ARRANGE
        var command = new UpdateProfileCommand
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            Description = "Desc",
            Enable = true,
            PrivilegeIds = new List<Guid>() 
        };

        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldNotHaveValidationErrorFor(c => c.PrivilegeIds);
    }
}
