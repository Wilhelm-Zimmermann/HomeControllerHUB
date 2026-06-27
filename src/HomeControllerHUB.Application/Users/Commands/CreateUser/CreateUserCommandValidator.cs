using FluentValidation;

namespace HomeControllerHUB.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty();        

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty();
        
        RuleFor(x => x.EstablishmentId)
            .NotEmpty();
        
        RuleFor(x => x)
            .Must(x => HasAny(x.EstablishmentIds) || HasAny(x.UserEstablishmentsIds))
            .WithMessage("At least one establishment must be informed.");
    }

    private static bool HasAny(IList<Guid>? ids)
    {
        return ids is { Count: > 0 };
    }
}
