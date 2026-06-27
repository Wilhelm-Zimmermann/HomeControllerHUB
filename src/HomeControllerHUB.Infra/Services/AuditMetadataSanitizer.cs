using System.Text.Json;
using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Infra.Services;

public class AuditMetadataSanitizer : IAuditMetadataSanitizer
{
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "confirmPassword",
        "token",
        "accessToken",
        "refreshToken",
        "apiKey",
        "authorization",
        "secret"
    };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public object? Sanitize(object? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        using var document = JsonSerializer.SerializeToDocument(metadata, SerializerOptions);
        return SanitizeElement(document.RootElement);
    }

    private static object? SanitizeElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeObject(element),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => ReadNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => null
        };
    }

    private static Dictionary<string, object?> SanitizeObject(JsonElement element)
    {
        var result = new Dictionary<string, object?>();

        foreach (var property in element.EnumerateObject())
        {
            if (SensitiveFields.Contains(property.Name))
            {
                continue;
            }

            result[property.Name] = SanitizeElement(property.Value);
        }

        return result;
    }

    private static object ReadNumber(JsonElement element)
    {
        if (element.TryGetInt64(out var longValue))
        {
            return longValue;
        }

        if (element.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        return element.GetDouble();
    }
}
