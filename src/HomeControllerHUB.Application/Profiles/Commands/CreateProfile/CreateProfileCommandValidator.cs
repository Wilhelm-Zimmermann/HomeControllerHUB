using FluentValidation;

namespace HomeControllerHUB.Application.Profiles.Commands.CreateProfile;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Enable)
            .NotNull();

        RuleForEach(x => x.PrivilegeIds)
            .NotEmpty()
            .When(x => x.PrivilegeIds != null);
    }
}
