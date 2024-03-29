using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sparcpoint.Common;

internal static class ObjectMapperHelpers
{
    internal static Type[] BasicTypes = new[]
    {
        typeof(bool),
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(float), typeof(double), typeof(decimal),
        typeof(DateTime), typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan),
        typeof(string)
    };

    internal static Func<string, object>[] Parsers = new Func<string, object>[]
    {
        (v) => bool.Parse(v),
        (v) => byte.Parse(v), (v) => short.Parse(v), (v) => int.Parse(v), (v) => long.Parse(v),
        (v) => float.Parse(v), (v) => double.Parse(v), (v) => decimal.Parse(v),
        (v) => DateTime.Parse(v), (v) => DateOnly.ParseExact(v, "o"), (v) => TimeOnly.ParseExact(v, "o"), (v) => TimeSpan.Parse(v),
        (v) => (v)
    };

    public static bool IsBasicType(this Type type)
        => BasicTypes.Contains(type);

    public static bool IsEnumerableType(this Type type)
        => typeof(IEnumerable).IsAssignableFrom(type);

    public static bool IsNullable(this Type type)
        => Nullable.GetUnderlyingType(type) != null;

    public static IEnumerable<PropertyInfo> GetEligibleProperties(this Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<IgnoreDataMemberAttribute>() == null)
            .ToArray()
        ;
    }

    public static object? ParseBasicValue(this Type type, string? value)
    {
        if (type == typeof(string))
            return value;

        if (string.IsNullOrWhiteSpace(value))
            return null;

        var index = Array.IndexOf(BasicTypes, type);
        return Parsers[index](value);
    }
}
