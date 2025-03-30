using Microsoft.Extensions.Localization;

namespace HomeControllerHUB.Globalization;

public class SharedResource : ISharedResource
{
    private readonly IStringLocalizer _localizer;

    public SharedResource(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string this[string index]
    {
        get
        {
            return _localizer[index];
        }
    }

    public string this[string index, params object[] arguments]
    {
        get
        {
            return _localizer[index, arguments];
        }
    }

    public string GetString(string key)
    {
        return _localizer.GetString(key);
    }

    public string GetString(string key, params object[] arguments)
    {
        return _localizer.GetString(key, arguments);
    }

    public string Message(string messageKey)
    {
        return _localizer.GetString(messageKey);
    }

    public string Message(string messageKey, params object[] arguments)
    {
        return _localizer.GetString(messageKey, arguments);
    }

    public string AlreadyExistsMessage(string entityKey)
    {
        return _localizer.GetString("EntityAlreadyExists", _localizer.GetString(entityKey));
    }

    public string AlreadyExistsWithParamMessage(string entityKey, string param)
    {
        return _localizer.GetString("EntityAlreadyExistsWithParam", _localizer.GetString(entityKey), _localizer.GetString(param));
    }

    public string EntityAlreadyHaveMessage(string entityKey, string param)
    {
        return _localizer.GetString("EntityAlreadyHave", _localizer.GetString(entityKey), _localizer.GetString(param));
    }

    public string InvalidParamMessage(string param)
    {
        return _localizer.GetString("InvalidParam", _localizer.GetString(param));
    }

    public string IsRequiredWhenHasAnotherParam(string requiredParam, string param)
    {
        return _localizer.GetString("IsRequiredWhenHasAnotherParam", _localizer.GetString(requiredParam), _localizer.GetString(param));
    }

    public string MustInformOneOfParams(params object[] arguments)
    {
        var requestFilters = string.Empty;
        foreach (var arg in arguments)
        {
            requestFilters += $"{arg}, ";
        }
        requestFilters = requestFilters.TrimEnd(',', ' ');

        return _localizer.GetString("MustInformOneOfParams", requestFilters);
    }

    public string MustInformOnlyOneParam(params object[] arguments)
    {
        var requestFilters = string.Empty;
        foreach (var arg in arguments)
        {
            requestFilters += $"{arg}, ";
        }
        requestFilters = requestFilters.TrimEnd(',', ' ');

        return _localizer.GetString("MustInformOnlyOneParam", requestFilters);
    }

    public string NotFoundMessage(string entityKey)
    {
        return _localizer.GetString(_localizer.GetString("EntityNotFound"), _localizer.GetString(entityKey));
    }

    public string ParamIsRequired(string param)
    {
        return _localizer.GetString("ParamIsRequired", _localizer.GetString(param));
    }

    public string ParamListCannotBeEmpty(string param)
    {
        return _localizer.GetString("ParamListCannotBeEmpty", _localizer.GetString(param));
    }

    public string ParamMustBeNumeric(string param)
    {
        return _localizer.GetString("ParamMustBeNumeric", _localizer.GetString(param));
    }

    public string RequiredFilters(params object[] arguments)
    {
        var requiredFilters = string.Empty;
        foreach (var arg in arguments)
        {
            requiredFilters += $"{arg}, ";
        }
        requiredFilters = requiredFilters.TrimEnd(',', ' ');

        return _localizer.GetString("RequiredFilters", requiredFilters);
    }
}
