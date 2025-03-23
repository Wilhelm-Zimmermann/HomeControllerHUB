using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HomeControllerHUB.Shared.Utils;

public static class StringExtensions
{
    public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
    {
        return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
    }

    public static int ToInt(this string value)
    {
        return Convert.ToInt32(value);
    }

    public static decimal ToDecimal(this string value)
    {
        return Convert.ToDecimal(value);
    }

    public static string ToNumeric(this int value)
    {
        return value.ToString("N0"); //"123,456"
    }

    public static string ToNumeric(this decimal value)
    {
        return value.ToString("N0");
    }

    public static string ToCurrency(this int value)
    {
        //fa-IR => current culture currency symbol => ریال
        //123456 => "123,123ریال"
        return value.ToString("C0");
    }

    public static string ToCurrency(this decimal value)
    {
        return value.ToString("C0");
    }

    public static string En2Fa(this string str)
    {
        return str.Replace("0", "۰")
            .Replace("1", "۱")
            .Replace("2", "۲")
            .Replace("3", "۳")
            .Replace("4", "۴")
            .Replace("5", "۵")
            .Replace("6", "۶")
            .Replace("7", "۷")
            .Replace("8", "۸")
            .Replace("9", "۹");
    }

    public static string Fa2En(this string str)
    {
        return str.Replace("۰", "0")
            .Replace("۱", "1")
            .Replace("۲", "2")
            .Replace("۳", "3")
            .Replace("۴", "4")
            .Replace("۵", "5")
            .Replace("۶", "6")
            .Replace("۷", "7")
            .Replace("۸", "8")
            .Replace("۹", "9")
            //iphone numeric
            .Replace("٠", "0")
            .Replace("١", "1")
            .Replace("٢", "2")
            .Replace("٣", "3")
            .Replace("٤", "4")
            .Replace("٥", "5")
            .Replace("٦", "6")
            .Replace("٧", "7")
            .Replace("٨", "8")
            .Replace("٩", "9");
    }

    public static string FixPersianChars(this string str)
    {
        return str.Replace("ﮎ", "ک")
            .Replace("ﮏ", "ک")
            .Replace("ﮐ", "ک")
            .Replace("ﮑ", "ک")
            .Replace("ك", "ک")
            .Replace("ي", "ی")
            .Replace(" ", " ")
            .Replace("\u200C", " ")
            .Replace("ھ", "ه");
    }

    public static string CleanString(this string str)
    {
        return str.Trim().FixPersianChars().Fa2En().NullIfEmpty();
    }

    public static string NullIfEmpty(this string str)
    {
        return str?.Length == 0 ? null : str;
    }

    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        // Replace all non-letter and non-digits with an underscore and lowercase the rest.
        string sample = string.Join(string.Empty, str.Select(c => Char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

        // Split the resulting string by underscore
        // Select first character, uppercase it and concatenate with the rest of the string
        var arr = sample
            .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}");

        // Join the resulting collection
        sample = string.Join(string.Empty, arr);

        return sample;
    }

    public static string Normalize(this string str)
    {
        return Regex.Replace(Regex.Replace(new string((from ch in str.Normalize(NormalizationForm.FormD)
                                                       where char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark
                                                       select ch).ToArray()), "[^0-9a-zA-Z\\s]+", "").ToUpper().Trim(), "\\s+", " ");
    }

    public static string NormalizeNumeric(this string str)
    {
        var regex = new Regex(@"\d+");

        StringBuilder builder = new StringBuilder();

        foreach (Match match in regex.Matches(str))
        {
            builder.Append(match.Value);
        }

        return builder.ToString();
    }

    public static string LimitChars(this string str, int maxLength)
    {
        if (str.Length <= maxLength)
        {
            return str;
        }

        // Encontrar a última palavra antes do comprimento máximo
        int lastWordIndex = str.LastIndexOf(' ', maxLength - 4);


        // Se não houver espaço, simplesmente cortar a string
        if (lastWordIndex == -1 || lastWordIndex >= maxLength - 4)
        {
            return str.Substring(0, maxLength - 3) + "...";
        }

        // Caso contrário, cortar na última palavra
        return str.Substring(0, lastWordIndex) + "...";
    }
}
