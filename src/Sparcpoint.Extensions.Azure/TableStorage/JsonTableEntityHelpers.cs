using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Azure.Data.Tables;
using SmartFormat;

namespace Sparcpoint.Extensions.Azure;

internal static class JsonTableEntityHelpers
{
    private static readonly ConcurrentDictionary<Type, PropertyDictionary> PropertyDictionaries = new();

    public static ITableEntity GetValue(object o)
    {
        var type = o.GetType();
        var props = GetProperties(type);

        var entity = new TableEntity();

        foreach (var prop in props)
        {
            if (!prop.Value.CanRead)
                continue;

            object? value = null;
            if (IsBasicType(prop.Value))
            {
                value = prop.Value.GetValue(o);
            }
            else if (TryGetNonArrayByteCollection(prop.Value, o, out var enumValue))
            {
                value = enumValue.ToArray();
            }
            else if (TryGetDecimal(prop.Value, o, out var decValue))
            {
                value = decValue.ToString();
            }
            else
            {
                // JSON Type
                var jsonValue = GetJsonValue(prop.Value, o);
                value = jsonValue;
            }

            entity.Add(prop.Key, value);
        }

        // Check for attribute for keys
        var attr = type.GetCustomAttribute<TableKeyAttribute>();
        if (attr != null)
        {
            var partitionKeyFormat = attr.PartitionKeyFormat;
            if (!string.IsNullOrWhiteSpace(partitionKeyFormat))
                entity["PartitionKey"] = Smart.Format(partitionKeyFormat, o);

            var rowKeyFormat = attr.RowKeyFormat;
            if (!string.IsNullOrWhiteSpace(rowKeyFormat))
                entity["RowKey"] = Smart.Format(rowKeyFormat, o);
        }

        return entity;
    }

    public static void SetValue(object o, TableEntity entity)
    {
        if (entity == null)
            return;

        var type = o.GetType();
        var props = GetProperties(type);

        foreach (var kv in entity)
        {
            string key = kv.Key;
            object? value = kv.Value;

            if (value == null)
                continue;

            if (!props.TryGetValue(key, out PropertyInfo? prop))
                continue;

            if (!prop.CanWrite)
                continue;

            if (IsBasicType(prop))
            {
                if (prop.PropertyType == typeof(DateTime) && value is DateTimeOffset dtoValue)
                {
                    prop.SetValue(o, dtoValue.DateTime);
                }
                else if (prop.PropertyType == typeof(DateTimeOffset) && value is DateTime dtValue)
                {
                    prop.SetValue(o, new DateTimeOffset(dtValue));
                }
                else
                {
                    prop.SetValue(o, value);
                }

            }
            else if (IsNonArrayByteCollection(prop))
            {

            }
            else if (IsDecimal(prop))
            {
                decimal? decValue = null;
                if (value != null)
                {
                    decValue = decimal.Parse((string)value);
                    prop.SetValue(o, decValue);
                }
            }
            else
            {
                string? jsonValue = (string)value;
                object? objValue = Deserialize(jsonValue, prop.PropertyType);
                if (objValue != null)
                    prop.SetValue(o, objValue);
            }
        }
    }

    public static PropertyDictionary GetProperties(Type type)
    {
        if (PropertyDictionaries.TryGetValue(type, out var dict))
            return dict;

        dict = new PropertyDictionary(type.GetProperties().ToDictionary(kv => kv.Name, kv => kv));
        PropertyDictionaries.TryAdd(type, dict);

        return dict;
    }
        

    private static readonly Type[] BaseTypes = [
        typeof(int),
        typeof(long),
        typeof(double),
        typeof(string),
        typeof(double),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(bool),
        typeof(byte[]),
        typeof(int?),
        typeof(long?),
        typeof(double?),
        typeof(DateTime?),
        typeof(bool?),
        typeof(DateTimeOffset?)
    ];

    private static readonly Type[] IgnoreTypes = [
        typeof(Type)
    ];

    private static bool IsBasicType(PropertyInfo info)
        => BaseTypes.Contains(info.PropertyType);

    private static bool TryGetNonArrayByteCollection(PropertyInfo info, object? value, [NotNullWhen(true)] out IEnumerable<byte>? data)
    {
        data = null;

        if (IsNonArrayByteCollection(info))
        {
            data = (IEnumerable<byte>?)info.GetValue(value);
            if (data != null)
                return true;
        }

        return false;
    }

    private static bool IsNonArrayByteCollection(PropertyInfo info)
        => typeof(IEnumerable<byte>).IsAssignableFrom(info.PropertyType);

    private static bool TryGetDecimal(PropertyInfo info, object? value, [NotNullWhen(true)] out decimal? data)
    {
        data = null;

        if (IsDecimal(info))
        {
            data = (decimal?)info.GetValue(value);
            if (data != null)
                return true;
        }

        return false;
    }

    private static bool IsDecimal(PropertyInfo info)
        => info.PropertyType == typeof(decimal) || info.PropertyType == typeof(decimal?);

    private static string? GetJsonValue(PropertyInfo info, object? value)
    {
        if (IgnoreTypes.Contains(info.PropertyType))
            return null;

        object? jsonValue = info.GetValue(value);
        if (jsonValue == null)
            return null;

        return Serialize(jsonValue);
    }

    private static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private static string Serialize(object value)
        => JsonSerializer.Serialize(value, Options);

    private static object? Deserialize(string json, Type type)
        => JsonSerializer.Deserialize(json, type, Options);
}
