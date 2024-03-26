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

        var priorityEntry = entries.Where(e => e.AccountId == accountId && e.Entry.Scope <= scope && e.Entry.Key == key).OrderByDescending(e => e.Entry.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Entry.Value;
    }
}
