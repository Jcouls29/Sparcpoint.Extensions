using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using SmartFormat;

namespace Sparcpoint.Extensions.Azure;

public abstract class JsonTableEntity
{
    private readonly Dictionary<string, PropertyInfo> _Properties;
    public JsonTableEntity()
    {
        _Properties = GetProperties();
    }

    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    internal ITableEntity GetValue()
    {
        var entity = new TableEntity();

        foreach (var prop in _Properties)
        {
            if (!prop.Value.CanRead)
                continue;

            object? value = null;
            if (IsBasicType(prop.Value))
            {
                value = prop.Value.GetValue(this);
            }
            else if (TryGetNonArrayByteCollection(prop.Value, out var enumValue))
            {
                value = enumValue.ToArray();
            }
            else if (TryGetDecimal(prop.Value, out var decValue))
            {
                value = decValue.ToString();
            }
            else
            {
                // JSON Type
                var jsonValue = GetJsonValue(prop.Value);
                value = jsonValue;
            }

            entity.Add(prop.Key, value);
        }

        // Check for attribute for keys
        var attr = GetType().GetCustomAttribute<TableKeyAttribute>();
        if (attr != null)
        {
            var partitionKeyFormat = attr.PartitionKeyFormat;
            if (!string.IsNullOrWhiteSpace(partitionKeyFormat))
                entity["PartitionKey"] = Smart.Format(partitionKeyFormat, this);

            var rowKeyFormat = attr.RowKeyFormat;
            if (!string.IsNullOrWhiteSpace(rowKeyFormat))
                entity["RowKey"] = Smart.Format(rowKeyFormat, this);
        }

        return entity;
    }

    internal void SetValue(TableEntity entity)
    {
        if (entity == null)
            return;

        foreach (var kv in entity)
        {
            string key = kv.Key;
            object? value = kv.Value;

            if (value == null)
                continue;

            if (!_Properties.TryGetValue(key, out PropertyInfo? prop))
                continue;

            if (!prop.CanWrite)
                continue;

            if (IsBasicType(prop))
            {
                if (prop.PropertyType == typeof(DateTime) && value is DateTimeOffset dtoValue)
                {
                    prop.SetValue(this, dtoValue.DateTime);
                }
                else if (prop.PropertyType == typeof(DateTimeOffset) && value is DateTime dtValue)
                {
                    prop.SetValue(this, new DateTimeOffset(dtValue));
                }
                else
                {
                    prop.SetValue(this, value);
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
                    prop.SetValue(this, decValue);
                }
            }
            else
            {
                string? jsonValue = (string)value;
                object? objValue = Deserialize(jsonValue, prop.PropertyType);
                if (objValue != null)
                    prop.SetValue(this, objValue);
            }
        }
    }

    private Dictionary<string, PropertyInfo> GetProperties()
        => GetType().GetProperties().ToDictionary(kv => kv.Name, kv => kv);

    private readonly Type[] BaseTypes = [
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

    private readonly Type[] IgnoreTypes = [
        typeof(Type)
    ];

    private bool IsBasicType(PropertyInfo info)
        => BaseTypes.Contains(info.PropertyType);

    private bool TryGetNonArrayByteCollection(PropertyInfo info, [NotNullWhen(true)] out IEnumerable<byte>? data)
    {
        data = null;

        if (IsNonArrayByteCollection(info))
        {
            data = (IEnumerable<byte>?)info.GetValue(this);
            if (data != null)
                return true;
        }

        return false;
    }

    private bool IsNonArrayByteCollection(PropertyInfo info)
        => typeof(IEnumerable<byte>).IsAssignableFrom(info.PropertyType);

    private bool TryGetDecimal(PropertyInfo info, [NotNullWhen(true)] out decimal? data)
    {
        data = null;

        if (IsDecimal(info))
        {
            data = (decimal?)info.GetValue(this);
            if (data != null)
                return true;
        }

        return false;
    }

    private bool IsDecimal(PropertyInfo info)
        => info.PropertyType == typeof(decimal) || info.PropertyType == typeof(decimal?);

    private string? GetJsonValue(PropertyInfo info)
    {
        if (IgnoreTypes.Contains(info.PropertyType))
            return null;

        object? value = info.GetValue(this);
        if (value == null)
            return null;

        return Serialize(value);
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