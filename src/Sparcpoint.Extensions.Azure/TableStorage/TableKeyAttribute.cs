using SmartFormat;
using System.Reflection;

namespace Sparcpoint.Extensions.Azure;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class TableKeyAttribute : Attribute
{
    public TableKeyAttribute(string partitionKeyFormat, string rowKeyFormat)
    {
        PartitionKeyFormat = partitionKeyFormat;
        RowKeyFormat = rowKeyFormat;
    }

    public string PartitionKeyFormat { get; }
    public string RowKeyFormat { get; }

    public static TableKeyAttribute? Get(Type type)
    {
        return type.GetCustomAttribute<TableKeyAttribute>();
    }

    public static TableKeyAttribute? Get<T>()
        => Get(typeof(T));

    public static string? FormatPartitionKey<T>(object? parameters = null)
        => FormatPartitionKey(typeof(T), parameters);

    public static string? FormatPartitionKey(Type type, object? parameters = null)
    {
        var attr = type.GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            return null;

        var key = attr.PartitionKeyFormat;
        if (key == null)
            return null;

        if (parameters == null)
            return key;

        return Smart.Format(key, parameters);
    }

    public static string? FormatRowKey<T>(object? parameters = null)
        => FormatRowKey(typeof(T), parameters);

    public static string? FormatRowKey(Type type, object? parameters = null)
    {
        var attr = type.GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            return null;

        var key = attr.RowKeyFormat;
        if (key == null)
            return null;

        if (parameters == null)
            return key;

        return Smart.Format(key, parameters);
    }
}
