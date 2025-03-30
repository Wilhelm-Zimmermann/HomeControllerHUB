namespace HomeControllerHUB.Globalization;

public interface ISharedResource
{

    string GetString(string key);

    string GetString(string key, params object[] arguments);

    string Message(string messageKey);

    string Message(string messageKey, params object[] arguments);

    string AlreadyExistsMessage(string entityKey);

    string AlreadyExistsWithParamMessage(string entityKey, string param);

    string EntityAlreadyHaveMessage(string entityKey, string param);

    string InvalidParamMessage(string param);

    string IsRequiredWhenHasAnotherParam(string requiredParam, string param);

    string MustInformOneOfParams(params object[] arguments);

    string MustInformOnlyOneParam(params object[] arguments);

    string NotFoundMessage(string entityKey);

    string ParamIsRequired(string param);

    string ParamListCannotBeEmpty(string param);

    string ParamMustBeNumeric(string param);

    string RequiredFilters(params object[] arguments);
}
