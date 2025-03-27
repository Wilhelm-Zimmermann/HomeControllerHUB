using System.Globalization;

namespace HomeControllerHUB.Globalization;

public static class CultureNames
{

    public const string Portuguese = "pt-BR";
    public const string English = "en-US";
    public const string Spanish = "es-ES";

    public readonly static CultureInfo PortugueseCulture = CultureInfo.CreateSpecificCulture(Portuguese);
    public readonly static CultureInfo EnglishCulture = CultureInfo.CreateSpecificCulture(English);
    public readonly static CultureInfo SpanishCulture = CultureInfo.CreateSpecificCulture(Spanish);

}
