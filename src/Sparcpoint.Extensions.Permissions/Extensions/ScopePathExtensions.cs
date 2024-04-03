namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class ScopePathExtensions
{
    public static PermissionEntry Allow(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => PermissionEntry.Allow(key, metadata);
    public static PermissionEntry Deny(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => PermissionEntry.Deny(key, metadata);

    public static AccountPermissionEntry Allow(this ScopePath scope, string accountId, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.Allow(accountId, key, scope, metadata);
    public static AccountPermissionEntry Deny(this ScopePath scope, string accountId, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.Deny(accountId, key, scope, metadata);

    public static AccountPermissionEntry AllowAll(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.AllowAll(key, scope, metadata);
    public static AccountPermissionEntry DenyAll(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.DenyAll(key, scope, metadata);
}

