namespace Sparcpoint.Extensions.Permissions;

public static class ScopePermissionCollectionExtensions
{
    public static async Task<IEnumerable<AccountPermissionEntry>> SetRangeAsync(this IScopePermissionCollection collection, ScopePath scope, Func<ScopePermissionsBuilder, ScopePermissionsBuilder> configure)
    {
        var builder = ScopePermissionsBuilder.Create(scope);
        configure(builder);
        var entries = builder.GetEntries();
        if (!entries.Any())
            return new List<AccountPermissionEntry>();

        await collection.SetRangeAsync(entries);

        return entries;
    }

    public static async Task<AccountPermissionEntry?> FindAsync(this IScopePermissionCollection collection, string accountId, string key, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var entry = await collection.Get(accountId, scope).FindAsync(key);
        if (entry == null)
            return null;

        return new AccountPermissionEntry(accountId, scope ?? ScopePath.RootScope, entry);
    }

    public static async Task<bool> IsAllowed(this IScopePermissionCollection collection, string accountId, string key, ScopePath? scope = null)
    {
        var found = await collection.FindAsync(accountId, key, scope);
        return found?.Entry?.Value == PermissionValue.Allow;
    }

    public static async Task<bool> IsDenied(this IScopePermissionCollection collection, string accountId, string key, ScopePath? scope = null)
    {
        var found = await collection.FindAsync(accountId, key, scope);
        return found?.Entry?.Value != PermissionValue.Allow;
    }
}

