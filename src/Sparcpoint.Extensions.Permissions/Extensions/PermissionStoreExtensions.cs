namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class PermissionStoreExtensions
{
    public static IAccountPermissionCollection Get(this IPermissionStore store, string accountId, ScopePath scope)
    {
        return store.Permissions.Get(accountId, scope);
    }
}

