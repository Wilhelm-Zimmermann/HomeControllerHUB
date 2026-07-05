using FluentAssertions;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Interceptors;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HomeControllerHUB.Application.Tests.AuditLogs.Interceptors;

public class AuthorizationBehaviourTests
{
    [Fact]
    public async Task Handle_ProtectedRequestWithoutRequiredPermission_ThrowsUnauthorized()
    {
        await using var context = CreateContext();
        var userId = await SeedUserAsync(context);
        var behaviour = CreateBehaviour<ProtectedRequest>(context, userId);

        var act = async () => await behaviour.Handle(new ProtectedRequest(), () => Task.FromResult("handled"), CancellationToken.None);

        await act.Should().ThrowAsync<AppError>()
            .Where(error => error.StatusCode == 401);
    }

    [Fact]
    public async Task Handle_ProtectedRequestWithRequiredPermission_ExecutesHandler()
    {
        await using var context = CreateContext();
        var userId = await SeedUserAsync(context, PrivilegeNames.UserRead, DomainNames.User, SecurityActionType.Read);
        var behaviour = CreateBehaviour<ProtectedRequest>(context, userId);

        var response = await behaviour.Handle(new ProtectedRequest(), () => Task.FromResult("handled"), CancellationToken.None);

        response.Should().Be("handled");
    }

    [Fact]
    public async Task Handle_ProtectedRequestWithPlatformAllPermission_ExecutesHandler()
    {
        await using var context = CreateContext();
        var userId = await SeedUserAsync(context, PrivilegeNames.All, DomainNames.User, SecurityActionType.All);
        var behaviour = CreateBehaviour<ProtectedRequest>(context, userId);

        var response = await behaviour.Handle(new ProtectedRequest(), () => Task.FromResult("handled"), CancellationToken.None);

        response.Should().Be("handled");
    }

    [Fact]
    public async Task Handle_PublicRequest_ExecutesHandlerWithoutAuthenticatedUser()
    {
        await using var context = CreateContext();
        var behaviour = CreateBehaviour<PublicRequest>(context, userId: null);

        var response = await behaviour.Handle(new PublicRequest(), () => Task.FromResult("handled"), CancellationToken.None);

        response.Should().Be("handled");
    }

    [Fact]
    public async Task Handle_UnauthorizedProtectedRequest_DoesNotExecuteHandler()
    {
        await using var context = CreateContext();
        var userId = await SeedUserAsync(context);
        var behaviour = CreateBehaviour<ProtectedRequest>(context, userId);
        var handlerExecuted = false;

        var act = async () => await behaviour.Handle(
            new ProtectedRequest(),
            () =>
            {
                handlerExecuted = true;
                return Task.FromResult("handled");
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<AppError>();
        handlerExecuted.Should().BeFalse();
    }

    private static ApplicationDbContext CreateContext()
    {
        var currentUserService = new Mock<ICurrentUserService>();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"authorization-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(
            options,
            new NormalizedInterceptor(),
            new BaseEntityInterceptor(currentUserService.Object));
    }

    private static AuthorizationBehaviour<TRequest, string> CreateBehaviour<TRequest>(
        ApplicationDbContext context,
        Guid? userId)
        where TRequest : IRequest<string>
    {
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(service => service.UserId).Returns(userId);
        currentUserService.Setup(service => service.Login).Returns("authorization-test");

        return new AuthorizationBehaviour<TRequest, string>(currentUserService.Object, context);
    }

    private static async Task<Guid> SeedUserAsync(
        ApplicationDbContext context,
        string? privilegeName = null,
        string? domainName = null,
        string? action = null)
    {
        var establishment = new Establishment
        {
            Id = Guid.NewGuid(),
            Code = "AUTH-EST",
            Name = "Authorization Test",
            NormalizedName = "AUTHORIZATION TEST",
            SiteName = "Authorization Test",
            NormalizedSiteName = "AUTHORIZATION TEST",
            Document = "12345678901",
            Enable = true,
            IsMaster = true
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Code = "AUTH-USER",
            UserName = "authorization-test",
            NormalizedUserName = "AUTHORIZATION-TEST",
            Name = "Authorization Test",
            NormalizedName = "AUTHORIZATION TEST",
            Email = "authorization@test.local",
            NormalizedEmail = "AUTHORIZATION@TEST.LOCAL",
            Login = "authorization-test",
            PasswordHash = "hash",
            Enable = true
        };

        context.Establishments.Add(establishment);
        context.Users.Add(user);

        if (privilegeName is not null && domainName is not null && action is not null)
        {
            var domain = new ApplicationDomain
            {
                Id = Guid.NewGuid(),
                Name = domainName,
                NormalizedName = domainName.ToUpperInvariant(),
                Enable = true
            };

            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                EstablishmentId = establishment.Id,
                Establishment = establishment,
                Name = "Authorization Profile",
                NormalizedName = "AUTHORIZATION PROFILE",
                Enable = true
            };

            var privilege = new Privilege
            {
                Id = Guid.NewGuid(),
                Name = privilegeName,
                NormalizedName = privilegeName.Replace("-", string.Empty).ToUpperInvariant(),
                Description = privilegeName,
                NormalizedDescription = privilegeName.ToUpperInvariant(),
                Actions = action,
                DomainId = domain.Id,
                Domain = domain,
                EstablishmentId = establishment.Id,
                Establishment = establishment,
                Enable = true
            };

            context.Domains.Add(domain);
            context.Profiles.Add(profile);
            context.Privilege.Add(privilege);
            context.ProfilePrivileges.Add(new ProfilePrivilege
            {
                ProfileId = profile.Id,
                Profile = profile,
                PrivilegeId = privilege.Id,
                Privilege = privilege
            });
            context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                User = user,
                ProfileId = profile.Id,
                Profile = profile
            });
        }

        await context.SaveChangesAsync();

        return user.Id;
    }

    [Authorize(Domain = DomainNames.User, Action = SecurityActionType.Read)]
    private sealed record ProtectedRequest : IRequest<string>;

    private sealed record PublicRequest : IRequest<string>;
}
