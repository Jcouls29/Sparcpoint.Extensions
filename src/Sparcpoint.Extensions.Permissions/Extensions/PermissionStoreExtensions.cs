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
}

