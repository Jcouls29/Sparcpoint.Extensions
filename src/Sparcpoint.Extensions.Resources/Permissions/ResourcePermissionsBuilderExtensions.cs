using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public static class ResourcePermissionsBuilderExtensions
{
    public static ResourcePermissionsBuilder CanReadData(this ResourcePermissionsBuilder builder)
        => builder.Allow(CommonPermissions.CanReadData);
    public static ResourcePermissionsBuilder CanWriteData(this ResourcePermissionsBuilder builder)
        => builder.Allow(CommonPermissions.CanWriteData);
    public static ResourcePermissionsBuilder CanReadPermissions(this ResourcePermissionsBuilder builder)
        => builder.Allow(CommonPermissions.CanReadPermissions);
    public static ResourcePermissionsBuilder CanWritePermissions(this ResourcePermissionsBuilder builder)
    => builder.Allow(CommonPermissions.CanWritePermissions);
}
