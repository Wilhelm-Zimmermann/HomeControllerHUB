using FluentValidation;

namespace HomeControllerHUB.Application.Profiles.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator() : base()
    {
        RuleFor(c => c.Id)
           .NotNull();

        RuleFor(c => c.Name)
            .NotNull();

        RuleFor(c => c.Description)
            .NotNull();

        RuleFor(v => v.Enable)
            .NotNull();

        RuleFor(c => c.PrivilegeIds)
            .NotEmpty();
    }
}
