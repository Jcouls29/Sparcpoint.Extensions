using SmartFormat;

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

    public string? GetContainerName(object? parameters = null)
    {
        if (ContainerName == null)
            return null;

        if (parameters != null)
            return Smart.Format(ContainerName, parameters);

        return ContainerName;
    }

    public string? GetPathFormat(object? parameters = null)
    {
        if (PathFormat == null)
            return null;

        if (parameters != null)
            return Smart.Format(PathFormat, parameters);

        return PathFormat;
    }
}
