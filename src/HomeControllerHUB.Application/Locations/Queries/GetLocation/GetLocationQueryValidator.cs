using FluentValidation;

namespace HomeControllerHUB.Application.Locations.Queries.GetLocation;

public class GetLocationQueryValidator : AbstractValidator<GetLocationQuery>
{
    public GetLocationQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
} 