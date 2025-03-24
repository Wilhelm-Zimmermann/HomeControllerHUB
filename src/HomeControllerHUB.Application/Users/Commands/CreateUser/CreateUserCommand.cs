using HomeControllerHUB.Domain.Models;
using MediatR;

namespace HomeControllerHUB.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<BaseEntityResponse>
{
    
};

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, BaseEntityResponse>
{
    public async Task<BaseEntityResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}