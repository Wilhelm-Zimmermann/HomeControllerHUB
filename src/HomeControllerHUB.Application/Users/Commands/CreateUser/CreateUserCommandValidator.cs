using FluentValidation;

namespace HomeControllerHUB.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty();        
        
        RuleFor(x => x.Password)
            .NotEmpty();
        
        RuleFor(x => x.EstablishmentId)
            .NotEmpty();
        
        RuleFor(x => x.UserEstablishmentsIds)
            .NotEmpty();
    }
}