using FluentValidation;
using FluentValidation.AspNetCore;

namespace HomeControllerHUB.Application.Profiles.Commands.CreateProfile;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Enable)
            .NotNull();

        RuleFor(x => x.PrivilegeIds)
            .NotNull();
    }
}