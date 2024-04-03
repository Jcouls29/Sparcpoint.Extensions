using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Extensions;

public static class PermissionEntryExtensions
{
    public static PermissionValue CalculatePermissionValue(this IEnumerable<AccountPermissionEntry> entries, ScopePath scope, string accountId, string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var priorityEntry = entries.Where(e => (e.AccountId == accountId || e.AccountId == AccountPermissionEntry.ALL_ACCOUNTS_ID) && e.Scope <= scope && e.Entry.Key == key).OrderByDescending(e => e.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Entry.Value;
    }

    public static IEnumerable<AccountPermissionEntry> CalculateView(this IEnumerable<AccountPermissionEntry> entries, ScopePath scope, bool includeRootScope = false, string[]? keys = null, string[]? accountIds = null)
    {
        var intermediate = entries
            .Where(e => e.Scope <= scope && (includeRootScope || e.Scope > ScopePath.RootScope))
            .Where(e => keys == null || keys.Contains(e.Entry.Key))
            .ToArray();
        var allPermissionKeys = (keys ?? intermediate.Select(c => c.Entry.Key)).Distinct();
        var allAccountIds = (accountIds ?? intermediate.Select(c => c.AccountId)).Distinct();

        var product = allAccountIds.CartesianProduct(allPermissionKeys);
        foreach(var k in product)
        {
            yield return new AccountPermissionEntry(k.Left, scope, new PermissionEntry(k.Right, intermediate.CalculatePermissionValue(scope, k.Left, k.Right), null));
        }
    }
}

