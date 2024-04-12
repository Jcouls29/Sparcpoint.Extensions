using Sparcpoint.Extensions.Permissions;
using System.Reflection;

namespace Sparcpoint.Extensions.Resources;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ResourcePermissionsAttribute : Attribute
{
    public static void SetPermissions(object resource, AccountPermissions permissions)
    {
        var props = resource.GetType().GetProperties().Where(p => p.GetCustomAttribute<ResourcePermissionsAttribute>() != null).ToArray();

        foreach(var prop in props)
        {
            if (prop.CanWrite && prop.PropertyType.IsAssignableFrom(typeof(AccountPermissions)))
            {
                prop.SetValue(resource, permissions);
            }
        }
    }
}


