using FluentValidation;
using FluentValidation.AspNetCore;

namespace HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;

public class CreateEstablishmentCommandValidator : AbstractValidator<CreateEstablishmentCommand>
{
    public CreateEstablishmentCommandValidator()
    {
        RuleFor(c => c.Document)
            .NotNull();

        RuleFor(c => c.SiteName)
            .NotNull();

        RuleFor(c => c.Name)
            .NotNull();

        RuleFor(v => v.Enable)
            .NotNull();

        RuleFor(v => v.IsMaster)
            .NotNull();
    }
}