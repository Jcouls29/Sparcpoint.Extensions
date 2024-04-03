namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionCollection : IScopePermissionCollection
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly object _LockObject;

    public InMemoryScopePermissionCollection(List<AccountPermissionEntry> entries, object lockObject)
    {
        _Entries = entries;
        _LockObject = lockObject;
    }

    public Task ClearAsync()
    {
        lock(_LockObject)
        {
            _Entries.Clear();
        }

        return Task.CompletedTask;
    }

    public Task<bool> ContainsAsync(AccountPermissionEntry entry)
    {
        EnsureEntryValid(entry);

        var result = _Entries.Any(k => k.AccountId == entry.AccountId && k.Scope == entry.Scope && k.Entry.Key == entry.Entry.Key);
        return Task.FromResult(result);
    }

    public async IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach(var e in _Entries)
        {
            yield return e;
        }
    }

    public Task SetRangeAsync(IEnumerable<AccountPermissionEntry> entries)
    {
        EnsureEntriesValid(entries);

        lock (_LockObject)
        {
            foreach(var e in entries)
            {
                var found = _Entries.FirstOrDefault(c => c.AccountId == e.AccountId && c.Scope == e.Scope && c.Entry.Key == e.Entry.Key);
                if (found != null)
                    _Entries.Remove(found);

                _Entries.Add(e);
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(IEnumerable<AccountPermissionEntry> entries)
    {
        EnsureEntriesValid(entries);

        lock (_LockObject)
        {
            foreach(var e in entries)
            {
                var found = _Entries.FirstOrDefault(c => c.AccountId == e.AccountId && c.Scope == e.Scope && c.Entry.Key == e.Entry.Key);
                if (found != null)
                    _Entries.Remove(found);
            }
        }

        return Task.CompletedTask;
    }

    private void EnsureEntriesValid(IEnumerable<AccountPermissionEntry> entries)
    {
        foreach (var e in entries)
        {
            EnsureEntryValid(e);
        }
    }

    private void EnsureEntryValid(AccountPermissionEntry entry)
    {
        Ensure.ArgumentNotNull(entry);
        Ensure.ArgumentNotNullOrWhiteSpace(entry.AccountId);
        Ensure.ArgumentNotNull(entry.Entry);
        Ensure.ArgumentNotNullOrWhiteSpace(entry.Entry.Key);
    }

    public async IAsyncEnumerable<AccountPermissionEntry> GetAsync(ScopePath scope)
    {
        var found = _Entries.Where(e => e.Scope == scope);
        foreach (var e in found)
            yield return e;
    }

    public IAccountPermissionCollection Get(string accountId, ScopePath? scope = null)
    {
        return new InMemoryAccountPermissionCollection(_Entries, _LockObject, accountId, scope ??  ScopePath.RootScope);
    }
}
