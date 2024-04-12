using System.Reflection;

namespace Sparcpoint.Extensions.Resources;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ResourceTypeAttribute : Attribute
{
    public ResourceTypeAttribute(string resourceType)
    {
        ResourceType = resourceType;
    }

    public string ResourceType { get; }

    public static string GetResourceType(Type? type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var attr = type.GetCustomAttribute<ResourceTypeAttribute>();
        if (attr != null)
        {
            if (string.IsNullOrWhiteSpace(attr.ResourceType))
                throw new InvalidOperationException("Resource type cannot be null, empty, or only whitespace.");

            return attr.ResourceType.Trim();
        }

        throw new InvalidOperationException("All Sparcpoint resource types must be decoarted with ResourceTypeAttribute.");
    }
}