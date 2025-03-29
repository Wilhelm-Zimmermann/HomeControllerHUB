using FluentValidation;

namespace HomeControllerHUB.Application.Users.Commands.AccessTokenUser;

public class AccessTokenUserCommandValidator : AbstractValidator<AccessTokenUserCommand>
{
    public AccessTokenUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();
        
        RuleFor(x => x.Password)
            .NotEmpty();
    }
}