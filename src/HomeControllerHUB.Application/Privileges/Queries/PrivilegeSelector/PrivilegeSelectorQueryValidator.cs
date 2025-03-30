using FluentValidation;

namespace HomeControllerHUB.Application.Privileges.Queries.PrivilegeSelector;

public class PrivilegeSelectorQueryValidator : AbstractValidator<PrivilegeSelectorQuery>
{
    public PrivilegeSelectorQueryValidator() : base()
    {
        RuleFor(q => q.SearchBy)
            .Must(ValidMinChar);
    }

    private bool ValidMinChar(string value)
    {
        return string.IsNullOrEmpty(value) || value.Length >= 3;
    }
}
