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
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var priorityEntry = entries.Where(e => e.Scope <= scope && e.Key == key).OrderByDescending(e => e.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Value;
    }

    public static PermissionValue CalculatePermissionValue(this IEnumerable<AccountPermissionEntry> entries, ScopePath scope, string accountId, string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var priorityEntry = entries.Where(e => (e.AccountId == accountId || e.AccountId == AccountPermissionEntry.ALL_ACCOUNTS_ID) && e.Entry.Scope <= scope && e.Entry.Key == key).OrderByDescending(e => e.Entry.Scope.Rank).FirstOrDefault();
        if (priorityEntry == null)
            return PermissionValue.None;

        return priorityEntry.Entry.Value;
    }

    public static IEnumerable<AccountPermissionEntry> CalculateView(this IEnumerable<AccountPermissionEntry> entries, ScopePath scope)
    {
        var intermediate = entries.Where(e => e.Entry.Scope <= scope).ToArray();
        var allPermissionKeys = intermediate.Select(c => c.Entry.Key).Distinct();
        var allAccountIds = intermediate.Select(c => c.AccountId).Distinct();

        var product = allAccountIds.CartesianProduct(allPermissionKeys);
        foreach(var k in product)
        {
            yield return new AccountPermissionEntry(k.Left, new PermissionEntry(k.Right, intermediate.CalculatePermissionValue(scope, k.Left, k.Right), scope, null));
        }
    }

    public static IEnumerable<PermissionEntry> CalculateView(this IEnumerable<PermissionEntry> entries, ScopePath scope)
    {
        var intermediate = entries.Where(e => e.Scope <= scope).ToArray();
        var allPermissionKeys = intermediate.Select(c => c.Key).Distinct();

        foreach (var k in allPermissionKeys)
        {
            yield return new PermissionEntry(k, intermediate.CalculatePermissionValue(scope, k), scope, null);
        }
    }
}

