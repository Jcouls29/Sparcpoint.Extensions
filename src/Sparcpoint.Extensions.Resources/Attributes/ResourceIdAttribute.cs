using System.Reflection;

namespace Sparcpoint.Extensions.Resources;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ResourceIdAttribute : Attribute
{
    public string? Format { get; set; } = null;

    public static void SetResourceId(object resource, ScopePath resourceId)
    {
        var props = resource.GetType().GetProperties().Where(p => p.GetCustomAttribute<ResourceIdAttribute>() != null).ToArray();

        foreach(var prop in props)
        {
            if (prop.PropertyType == typeof(ScopePath) && prop.CanWrite)
            {
                prop.SetValue(resource, resourceId);
            }
            else if (prop.PropertyType == typeof(string) && prop.CanWrite)
            {
                prop.SetValue(resource, resourceId.ToString());
            }
        }
    }

    public static string? GetFormat(Type type)
    {
        var found = type
            .GetProperties()
            .Select(p => p.GetCustomAttribute<ResourceIdAttribute>())
            .FirstOrDefault(attr => attr != null);

        if (found == null)
            return null;

        if (string.IsNullOrWhiteSpace(found.Format))
            return null;

        return found.Format;
    }
}


