namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class PermissionStoreExtensions
{
    public static IAccountPermissionCollection Get(this IPermissionStore store, string accountId, ScopePath scope)
    {
        return store.Permissions.Get(accountId, scope);
    }

    public static async Task SetRangeAsync(this IPermissionStore store, ScopePath initialScope, Func<ScopePermissionsBuilder, ScopePermissionsBuilder> configure)
    {
        var builder = ScopePermissionsBuilder.Create(initialScope);
        configure(builder);
        var entries = builder.GetEntries();
        if (!entries.Any())
            return;

        await store.Permissions.SetRangeAsync(entries);
    }

    public static async Task<bool> IsAllowedAsync(this IPermissionStore store, string accountId, string key, ScopePath? scope = null, bool includeRootScope = false)
        => await IsValueAsync(store, accountId, key, PermissionValue.Allow, scope, includeRootScope);

    public static async Task<bool> IsDeniedAsync(this IPermissionStore store, string accountId, string key, ScopePath? scope = null, bool includeRootScope = false)
        => await IsValueAsync(store, accountId, key, PermissionValue.Deny, scope, includeRootScope);

    public static async Task<bool> IsValueAsync(this IPermissionStore store, string accountId, string key, PermissionValue value, ScopePath? scope = null, bool includeRootScope = false)
    {
        var view = store.GetView(scope ?? ScopePath.RootScope, includeRootScope, new[] { key });
        var found = await view.FirstOrDefaultAsync(x => x.AccountId == accountId && x.Entry.Key == key);

        if (found == null)
            return value == PermissionValue.None;

        return found.Entry.Value == value;
    }
}

