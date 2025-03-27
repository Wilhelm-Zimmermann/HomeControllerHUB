using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HomeControllerHUB.Application.Users.Commands.CreateUser;

[Authorize(Domain = DomainNames.User, Action = SecurityActionType.Create)]
public record CreateUserCommand : IRequest<BaseEntityResponse>
{
    public string Login { get; init; }
    public string Password { get; init; }
    public string Name { get; init; }
    public string Document { get; init; }
    public Guid EstablishmentId { get; init; }
    public IList<Guid>? UserEstablishmentsIds { get; private set; }
    public IList<Guid> UserProfilesIds { get; private set; }
};

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, BaseEntityResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    
    public async Task<BaseEntityResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid(),
            Login = request.Login,
            Name = request.Name,
            Document = request.Document,
            EstablishmentId = request.EstablishmentId,
        };

        foreach (var profileId in request.UserProfilesIds)
        {
            var userProfile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProfileId = profileId,
            };

            _context.UserProfiles.Add(userProfile);
        }

        if (request?.UserEstablishmentsIds?.Count > 0)
        {
            foreach (var establishmentId in request.UserEstablishmentsIds)
            {
                var userEstablishment = new UserEstablishment
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EstablishmentId = establishmentId,
                };
                
                _context.UserEstablishments.Add(userEstablishment);
            }
        }
        
        await _userManager.CreateAsync(user, request.Password);
        await _context.SaveChangesAsync(cancellationToken);
        return new BaseEntityResponse()
        {
            Id = user.Id,
        };
    }
}