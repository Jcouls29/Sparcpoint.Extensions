using Sparcpoint.Common.Extensions;

namespace Sparcpoint.Extensions.Resources;

public class ResourcePermissions : List<ResourcePermissionEntry>
{
    public ResourcePermissions() : base() { }
    public ResourcePermissions(IEnumerable<ResourcePermissionEntry> entries) : base(entries) { }

    public static ResourcePermissions With(string accountId, Func<ResourcePermissionsBuilder, ResourcePermissionsBuilder> config)
    {
        var builder = new ResourcePermissionsBuilder(accountId);
        config(builder);

        return builder;
    }

    /// <summary>
    /// Merging left will overwrite any values associated with this set of permissions with the values in right. Matches on Account Id and Permission Key.
    /// </summary>
    /// <param name="right"></param>
    /// <returns></returns>
    public ResourcePermissions MergeLeft(ResourcePermissions? right)
    {
        if (right == null)
            return this;

        var permissions = new ResourcePermissions(right);
        foreach(var entry in this)
        {
            if (!permissions.Any(e => e.AccountId == entry.AccountId && e.Permission.Key == entry.Permission.Key))
                permissions.Add(entry);
        }

        return permissions;
    }

    public ResourcePermissions MergeLeft(params ResourcePermissions[] right)
    {
        var result = this;
        foreach(var entry in right)
        {
            result = result.MergeLeft(entry);
        }
        return result;
    }

    public static ResourcePermissions Merge(params ResourcePermissions[] permissionSets)
    {
        if (permissionSets == null || !permissionSets.Any())
            return new();

        if (permissionSets.Length == 1)
            return permissionSets[0];

        return permissionSets[0].MergeLeft(permissionSets.Slice(1));
    }

    public static ResourcePermissions WithOwnerPermissions(string accountId)
        => With(accountId, (b) => b.CanReadData().CanWriteData().CanReadPermissions().CanWritePermissions());
}
