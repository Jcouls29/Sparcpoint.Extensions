using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public static class ResourcePermissionsBuilderExtensions
{
    public static ResourcePermissionsBuilder CanRead(this ResourcePermissionsBuilder builder)
        => builder.Allow(CommonPermissions.CanRead);
    public static ResourcePermissionsBuilder CanWrite(this ResourcePermissionsBuilder builder)
        => builder.Allow(CommonPermissions.CanWrite);
}
