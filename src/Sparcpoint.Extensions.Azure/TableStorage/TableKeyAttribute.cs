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

    public static string? FormatPartitionKey(Type type, object value)
    {
        var attr = type.GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            return null;

        var key = attr.PartitionKeyFormat;
        if (key == null)
            return null;

        return Smart.Format(key, value);
    }

    public static string? FormatRowKey(Type type, object value)
    {
        var attr = type.GetCustomAttribute<TableKeyAttribute>();
        if (attr == null)
            return null;

        var key = attr.RowKeyFormat;
        if (key == null)
            return null;

        return Smart.Format(key, value);
    }
}
