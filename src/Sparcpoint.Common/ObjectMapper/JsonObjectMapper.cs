using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sparcpoint.Common;

public sealed class JsonObjectMapper : IObjectMapper
{
    private readonly JsonSerializerOptions _Options;
    public JsonObjectMapper()
    {
        _Options = new JsonSerializerOptions();
        _Options.Converters.Add(new DateOnlyConverter());
        _Options.Converters.Add(new TimeOnlyConverter());
        _Options.IgnoreReadOnlyFields = true;
        _Options.IgnoreReadOnlyProperties = true;
        _Options.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString;
        _Options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }

    public void Map<T>(T obj, IDictionary<string, string?> results)
    {
        Ensure.ArgumentNotNull(obj);

        var props = obj.GetType().GetEligibleProperties();

        foreach(var p in props)
        {
            var value = p.GetValue(obj);
            if (value == null)
            {
                results.Add(p.Name, null);
                continue;
            }

            if (p.PropertyType.IsBasicType())
            {
                if (value is DateOnly d)
                    results.Add(p.Name, d.ToString("o"));
                else if (value is TimeOnly t)
                    results.Add(p.Name, t.ToString("o"));
                else
                    results.Add(p.Name, value.ToString()!);

                continue;
            }

            if (p.PropertyType.IsNullable())
            {
                if (value == null)
                {
                    results.Add(p.Name, null);
                    continue;
                }

                if (value is DateOnly d)
                    results.Add(p.Name, d.ToString("o"));
                else if (value is TimeOnly t)
                    results.Add(p.Name, t.ToString("o"));
                else
                    results.Add(p.Name, value.ToString()!);

                continue;
            }

            // Complex Object
            var json = JsonSerializer.Serialize(value, p.PropertyType);
            results.Add(p.Name, json);
        }
    }

    public void Map<T>(IReadOnlyDictionary<string, string?> dictionary, T result) 
    {
        var props = typeof(T).GetEligibleProperties();

        foreach(var p in props)
        {
            var type = p.PropertyType;
            if (!dictionary.TryGetValue(p.Name, out string? value))
                continue;

            if (value == null)
            {
                p.SetValue(result, null);
                continue;
            }

            if (type.IsBasicType())
            {
                p.SetValue(result, ObjectMapperHelpers.ParseBasicValue(type, value));
            }
            else if (type.IsNullable() && Nullable.GetUnderlyingType(type)!.IsBasicType())
            {
                if (value == null)
                    p.SetValue(result, null);
                else
                    p.SetValue(result, ObjectMapperHelpers.ParseBasicValue(Nullable.GetUnderlyingType(type)!, value));
            } 
            else
            {
                var objValue = JsonSerializer.Deserialize(value, type, _Options);
                p.SetValue(result, objValue);
            }
        }
    }

    public static JsonObjectMapper Instance { get; } = new JsonObjectMapper();
}
