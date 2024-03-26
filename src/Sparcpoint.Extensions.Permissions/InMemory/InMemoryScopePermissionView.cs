using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionView : IScopePermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;

    public InMemoryScopePermissionView(List<AccountPermissionEntry> entries, ScopePath currentScope)
    {
        _Entries = entries;
        CurrentScope = currentScope;
    }

    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var intermediate = _Entries.Where(e => e.Entry.Scope <= CurrentScope).ToArray();
        var allPermissionKeys = intermediate.Select(c => c.Entry.Key).Distinct();
        var allAccountIds = intermediate.Select(c => c.AccountId).Distinct();

        var product = CartesianProduct(allAccountIds, allPermissionKeys);
        var view = product
            .Select(k => new AccountPermissionEntry(k.AccountId, new PermissionEntry(k.Key, intermediate.CalculatePermissionValue(CurrentScope, k.AccountId, k.Key), CurrentScope, null)))
            .ToList();

        return new SynchronousAsyncEnumerator<AccountPermissionEntry>(view.GetEnumerator());
    }

    private IEnumerable<AccountPermissionKey> CartesianProduct(IEnumerable<string> accountIds, IEnumerable<string> keys)
    {
        List<AccountPermissionKey> result = new();
        foreach (var accountId in accountIds)
        {
            foreach (var key in keys)
            {
                result.Add(new AccountPermissionKey { AccountId = accountId, Key = key });
            }
        }

        return result;
    }

    private struct AccountPermissionKey
    {
        public string AccountId { get; set; }
        public string Key { get; set; }
    }
}