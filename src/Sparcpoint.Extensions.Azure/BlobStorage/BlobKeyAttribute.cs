using SmartFormat;
using System.Reflection;

namespace Sparcpoint.Extensions.Azure;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class BlobKeyAttribute : Attribute
{
    public BlobKeyAttribute(string containerName, string pathFormat)
    {
        ContainerName = containerName;
        PathFormat = pathFormat;
    }

    public string ContainerName { get; }
    public string PathFormat { get; }

    public string? FormatContainerName(object? parameters = null)
    {
        if (ContainerName == null)
            return null;

        if (parameters != null)
            return Smart.Format(ContainerName, parameters);

        return ContainerName;
    }

    public string? FormatPath(object? parameters = null)
    {
        if (PathFormat == null)
            return null;

        if (parameters != null)
            return Smart.Format(PathFormat, parameters);

        return PathFormat;
    }

    public static BlobKeyAttribute? Get<T>()
        => Get(typeof(T));

    public static BlobKeyAttribute? Get(Type type)
        => type.GetCustomAttribute<BlobKeyAttribute>();

    public static string? FormatContainerName<T>(object? parameters = null)
        => FormatContainerName(typeof(T), parameters);

    public static string? FormatContainerName(Type type, object? parameters = null)
    {
        var attr = Get(type);
        if (attr == null) 
            return null;

        return attr.FormatContainerName(parameters);
    }

    public static string? FormatPath<T>(object? parameters = null)
        => FormatPath(typeof(T), parameters);

    public static string? FormatPath(Type type, object? parameters = null)
    {
        var attr = Get(type);
        if (attr == null)
            return null;

        return attr.FormatPath(parameters);
    }
}
