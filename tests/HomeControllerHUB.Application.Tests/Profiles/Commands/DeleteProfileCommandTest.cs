using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Commands.DeleteProfile;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using MediatR;
using Moq;

namespace HomeControllerHUB.Application.Tests.Profiles.Commands;

public class DeleteProfileCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<IMediator> _mediatorMock;

    public DeleteProfileCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task Delete_Should_Succeed_AndRemoveAssociations()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var profile = new Profile { Name = "ToDelete", EstablishmentId = establishment.Id };
        var user = new ApplicationUser { Name = "Test User", Login = "test", Email = "test@test.com", NormalizedName = "TEST", PasswordHash = "hash", EstablishmentId = establishment.Id };
        var userProfile = new UserProfile { Profile = profile, User = user };
        
        _context.Profiles.Add(profile);
        _context.Users.Add(user);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();
        
        var command = new DeleteProfileCommand(profile.Id);
        var handler = new DeleteProfilesCommandHandler(_context, _resourceMock.Object, _mediatorMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var profileInDb = await _context.Profiles.FindAsync(profile.Id);
        profileInDb.Should().BeNull();
        
        var userProfileInDb = await _context.UserProfiles.FindAsync(userProfile.Id);
        userProfileInDb.Should().BeNull();
    }
}
