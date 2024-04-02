using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryAccountPermissionView : IAccountPermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly object _LockObject;

    public InMemoryAccountPermissionView(List<AccountPermissionEntry> entries, object lockObject, string accountId, ScopePath currentScope)
    {
        _Entries = entries;
        _LockObject = lockObject;

        AccountId = accountId;
        CurrentScope = currentScope;
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var intermediate = _Entries.Where(e => e.AccountId == AccountId && e.Scope <= CurrentScope).ToArray();
        var allPermissionKeys = intermediate.Select(c => c.Entry.Key).Distinct();

        var view = allPermissionKeys.Select(k => new PermissionEntry(k, intermediate.CalculatePermissionValue(CurrentScope, AccountId, k), null)).ToList();
        return new SynchronousAsyncEnumerator<PermissionEntry>(view.GetEnumerator());
    }
}