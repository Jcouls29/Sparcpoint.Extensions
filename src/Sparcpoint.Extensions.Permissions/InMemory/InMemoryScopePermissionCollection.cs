﻿namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionCollection : IScopePermissionCollection
{
    private readonly List<AccountPermissionEntry> _Entries;

    public InMemoryScopePermissionCollection(List<AccountPermissionEntry> entries, ScopePath currentScope)
    {
        _Entries = entries;
        CurrentScope = currentScope;
    }

    public ScopePath CurrentScope { get; }

    public Task ClearAsync()
    {
        var removeValues = ScopedView.ToArray();
        foreach (var value in removeValues)
            _Entries.Remove(value);

        return Task.CompletedTask;
    }

    public Task<bool> ContainsAsync(string accountId, string key)
    {
        var result = ScopedView.Any(k => k.AccountId == accountId && k.Entry.Key == key);
        return Task.FromResult(result);
    }

    public Task<IAsyncEnumerable<AccountPermissionEntry>> GetAsync(string key)
    {
        var found = ScopedView.Where(c => c.Entry.Key == key).ToList();
        var result = new SynchronousAsyncEnumerable<AccountPermissionEntry>(found);

        return Task.FromResult((IAsyncEnumerable<AccountPermissionEntry>)result);
    }

    public IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var view = ScopedView.ToList().GetEnumerator();
        return new SynchronousAsyncEnumerator<AccountPermissionEntry>(view);
    }

    public Task RemoveAsync(string accountId, string key)
    {
        var found = ScopedView.FirstOrDefault(c => c.AccountId == accountId && c.Entry.Key == key);
        if (found != null)
            _Entries.Remove(found);

        return Task.CompletedTask;
    }

    public Task SetAsync(string accountId, string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(key);

        var found = ScopedView.FirstOrDefault(c => c.AccountId == accountId && c.Entry.Key == key);
        if (found != null)
            _Entries.Remove(found);

        _Entries.Add(new AccountPermissionEntry(accountId, new PermissionEntry(key, value, CurrentScope, metadata)));
        return Task.CompletedTask;
    }

    private IEnumerable<AccountPermissionEntry> ScopedView => _Entries.Where(e => e.Entry.Scope == CurrentScope);
}
