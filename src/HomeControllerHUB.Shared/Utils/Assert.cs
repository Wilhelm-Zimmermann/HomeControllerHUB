using System.Collections;

namespace HomeControllerHUB.Shared.Utils;

public static class Assert
{
    public static void NotNull<T>(T obj, string name, string? message = null) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException($"{name} : {typeof(T)}", message);
        }
    }

    public static void NotNull<T>(T? obj, string name, string? message = null) where T : struct
    {
        if (!obj.HasValue)
        {
            throw new ArgumentNullException($"{name} : {typeof(T)}", message);
        }
    }

    public static void NotEmpty<T>(T obj, string name, string? message = null, T? defaultValue = null) where T : class
    {
        if (obj == defaultValue
            || (obj is string str && string.IsNullOrWhiteSpace(str))
            || (obj is IEnumerable list && !list.Cast<object>().Any()))
        {
            throw new ArgumentException("Argument is empty : " + message, $"{name} : {typeof(T)}");
        }
    }

    public static void IsTrue(bool expression, string name, string? message = null)
    {
        if (!expression)
        {
            throw new ArgumentException("Argument is False: " + message, $"{name}");
        }
    }

    public static void IsFalse(bool expression, string name, string? message = null)
    {
        if (expression)
        {
            throw new ArgumentException("Argument is True: " + message, $"{name}");
        }
    }
}