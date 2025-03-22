using HomeControllerHUB.Infra.Services;
using MediatR;

namespace HomeControllerHUB.Application.Users.Commands;

public record CreateUserCommand : IRequest
{
    
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly ApiUserManager _userManager;
    
    public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // _userManager.CreateAsync();
        throw new NotImplementedException();
    }
}