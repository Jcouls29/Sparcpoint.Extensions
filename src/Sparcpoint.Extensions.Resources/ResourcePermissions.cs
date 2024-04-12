namespace Sparcpoint.Extensions.Resources;

public class ResourcePermissions : List<ResourcePermissionEntry>
{
    public static ResourcePermissions With(string accountId, Func<ResourcePermissionsBuilder, ResourcePermissionsBuilder> config)
    {
        var builder = new ResourcePermissionsBuilder(accountId);
        config(builder);

        return builder;
    }

    public static ResourcePermissions WithOwnerPermissions(string accountId)
        => With(accountId, (b) => b.CanRead().CanWrite());
}
