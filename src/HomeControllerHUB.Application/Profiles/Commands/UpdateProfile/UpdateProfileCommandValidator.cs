using FluentValidation;

namespace HomeControllerHUB.Application.Profiles.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator() : base()
    {
        RuleFor(c => c.Id)
           .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty();

        RuleFor(c => c.Description)
            .NotNull();

        RuleFor(v => v.Enable)
            .NotNull();

        RuleForEach(c => c.PrivilegeIds)
            .NotEmpty()
            .When(c => c.PrivilegeIds != null);
    }
}
