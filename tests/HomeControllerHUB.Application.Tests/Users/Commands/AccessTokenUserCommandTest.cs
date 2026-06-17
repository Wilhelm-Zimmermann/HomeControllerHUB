using FluentAssertions;
using HomeControllerHUB.Application.Users.Commands.AccessTokenUser;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Infra.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HomeControllerHUB.Application.Tests.Users.Commands;

public class AccessTokenUserCommandTest : TestConfigs
{
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly ApplicationSettings _appSettings;

    public AccessTokenUserCommandTest()
    {
        _jwtServiceMock = new Mock<IJwtTokenService>();
        _resourceMock = new Mock<ISharedResource>();
        _appSettings = new ApplicationSettings { JwtSettings = new JwtSettings { AppName = "TestApp", RefreshTokenName = "TestToken" } };
    }

    private ApiUserManager CreateUserManager()
    {
        var store = new UserStore(_context);
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passValidators = new List<IPasswordValidator<ApplicationUser>>();
        
        return new ApiUserManager(store, options.Object, _context,
            new PasswordHasher<ApplicationUser>(), userValidators, passValidators,
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }
    
    [Fact]
    public async Task AccessToken_Should_ReturnTokens_WhenCredentialsAreValid()
    {
        // ARRANGE
        var userManager = CreateUserManager();
        var user = new ApplicationUser { UserName = "test", Login = "test", Email = "t@t.com", Name="Test", EmailConfirmed = true, Establishment = await CreateEstablishment() };
        await userManager.CreateAsync(user, "Password123!");

        _jwtServiceMock.Setup(s => s.GenerateAsync(It.IsAny<ApplicationUser>(), It.IsAny<Establishment>(), null))
            .ReturnsAsync(new AccessTokenEntry { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" });

        var command = new AccessTokenUserCommand { Username = "test", Password = "Password123!" };
        var handler = new AccessTokenUserCommandHandler(_jwtServiceMock.Object, userManager, _appSettings, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
    }

    [Fact]
    public async Task AccessToken_Should_ThrowAppError_WhenPasswordIsInvalid()
    {
        // ARRANGE
        var userManager = CreateUserManager();
        var user = new ApplicationUser { UserName = "test", Login = "test", Email = "t@t.com", Name="Test", EmailConfirmed = true, Establishment = await CreateEstablishment() };
        await userManager.CreateAsync(user, "Password123!");

        var command = new AccessTokenUserCommand { Username = "test", Password = "WrongPassword!" };
        var handler = new AccessTokenUserCommandHandler(_jwtServiceMock.Object, userManager, _appSettings, _resourceMock.Object);
        _resourceMock.Setup(r => r.Message(It.IsAny<string>())).Returns("Invalid login.");
        
        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        
        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 400);
    }
}
