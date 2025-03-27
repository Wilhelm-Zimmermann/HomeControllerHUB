using FluentValidation;

namespace HomeControllerHUB.Application.Users.Commands.AccessTokenUser;

public class AccessTokenUserCommandValidator : AbstractValidator<AccessTokenUserCommand>
{
    public AccessTokenUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty();
        
        RuleFor(x => x.Password)
            .NotEmpty();
    }
}