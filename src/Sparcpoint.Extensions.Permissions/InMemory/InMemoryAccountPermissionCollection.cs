using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryAccountPermissionCollection : IAccountPermissionCollection
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly object _LockObject;

    public InMemoryAccountPermissionCollection(List<AccountPermissionEntry> entries, object lockObject, string accountId, ScopePath currentScope)
    {
        _Entries = entries;
        _LockObject = lockObject;

        AccountId = accountId;
        CurrentScope = currentScope;
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public Task SetRangeAsync(IEnumerable<PermissionEntry> entries)
    {
        foreach(var e in entries)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(e.Key);
        }
        

        lock (_LockObject)
        {
            foreach(var e in entries)
            {
                var found = ScopedView.FirstOrDefault(c => c.Entry.Key == e.Key);
                if (found != null)
                    _Entries.Remove(found);

                _Entries.Add(new AccountPermissionEntry(AccountId, CurrentScope, e));
            }
        }
        
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        lock(_LockObject)
        {
            var removeValues = ScopedView.ToArray();
            foreach (var value in removeValues)
                _Entries.Remove(value);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ContainsAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var result = ScopedView.Any(k => k.Entry.Key == key);
        return Task.FromResult(result);
    }

    public Task<PermissionEntry?> FindAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var found = ScopedView.FirstOrDefault(c => c.Entry.Key == key);
        if (found == null)
            return Task.FromResult((PermissionEntry?)null);

        return Task.FromResult((PermissionEntry?)found.Entry);
    }

    public IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var view = ScopedView.Select(c => c.Entry).ToList().GetEnumerator();
        return new SynchronousAsyncEnumerator<PermissionEntry>(view);
    }

    public Task RemoveAsync(IEnumerable<string> keys)
    {
        foreach(var k in keys)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(k);
        }

        lock (_LockObject)
        {
            foreach(var k in keys)
            {
                var found = ScopedView.FirstOrDefault(c => c.Entry.Key == k);
                if (found != null)
                    _Entries.Remove(found);
            }
        }

        return Task.CompletedTask;
    }

    private IEnumerable<AccountPermissionEntry> ScopedView => _Entries.Where(e => e.AccountId == AccountId && e.Scope == CurrentScope);
}