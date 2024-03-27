using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class PermissionEntryExtensions
{
    public static PermissionValue CalculatePermissionValue(this IEnumerable<PermissionEntry> entries, ScopePath scope, string key)
    {
        Assertions.NotEmptyOrWhitespace(key);

        var priorityEntry = entries.Where(e => e.Scope <= scope && e.Key == key).OrderByDescending(e => e.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Value;
    }

    public static PermissionValue CalculatePermissionValue(this IEnumerable<AccountPermissionEntry> entries, ScopePath scope, string accountId, string key)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(key);

        var priorityEntry = entries.Where(e => (e.AccountId == accountId || e.AccountId == AccountPermissionEntry.ALL_ACCOUNTS_ID) && e.Entry.Scope <= scope && e.Entry.Key == key).OrderByDescending(e => e.Entry.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Entry.Value;
    }
}

public static class ScopePathExtensions
{
    public static PermissionEntry Allow(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => PermissionEntry.Allow(key, scope, metadata);
    public static PermissionEntry Deny(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => PermissionEntry.Deny(key, scope, metadata);

    public static AccountPermissionEntry Allow(this ScopePath scope, string accountId, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.Allow(accountId, key, scope, metadata);
    public static AccountPermissionEntry Deny(this ScopePath scope, string accountId, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.Deny(accountId, key, scope, metadata);

    public static AccountPermissionEntry AllowAll(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.AllowAll(key, scope, metadata);
    public static AccountPermissionEntry DenyAll(this ScopePath scope, string key, Dictionary<string, string>? metadata = null)
        => AccountPermissionEntry.DenyAll(key, scope, metadata);
}

