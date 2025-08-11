using FluentValidation;

namespace HomeControllerHUB.Application.Establishments.Commands.UpdateEstablishment;

public class UpdateEstablishmentCommandValidator : AbstractValidator<UpdateEstablishmentCommand>
{
    public UpdateEstablishmentCommandValidator() : base()
    {
        RuleFor(c => c.Id)
            .NotEmpty();

        RuleFor(c => c.Document)
            .NotNull();

        RuleFor(c => c.SiteName)
            .NotNull();

        RuleFor(c => c.Name)
            .NotNull();

        RuleFor(v => v.Enable)
            .NotNull();
    }
}
