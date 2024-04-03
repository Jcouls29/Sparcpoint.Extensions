namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class AccountPermissionCollectionExtensions
{
    public static async Task<bool> IsAllowedDirect(this IAccountPermissionCollection collection, string key)
    {
        if (!await collection.ContainsAsync(key))
            return false;

        var found = await collection.FindAsync(key);
        return found?.Value == PermissionValue.Allow;
    }

    public static async Task<bool> IsDeniedDirect(this IAccountPermissionCollection collection, string key)
    {
        if (!await collection.ContainsAsync(key))
            return false;

        var found = await collection.FindAsync(key);
        return found == null || found.Value != PermissionValue.Allow;
    }

    public static async Task SetAsync(this IAccountPermissionCollection collection, string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Ensure.NotNullOrWhiteSpace(key);

        await collection.SetRangeAsync(new[] 
        {
            new PermissionEntry(key, value, metadata)
        });
    }

    public static async Task AddRangeAsync(this IAccountPermissionCollection collection, params PermissionEntry[] entries)
        => await collection.SetRangeAsync(entries);

    public static async Task RemoveAsync(this IAccountPermissionCollection collection, params string[] keys)
        => await collection.RemoveAsync(keys);
}

