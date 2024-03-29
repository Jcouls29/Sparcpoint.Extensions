﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryAccountPermissionCollection : IAccountPermissionCollection
{
    private readonly List<AccountPermissionEntry> _Entries;
    public InMemoryAccountPermissionCollection(List<AccountPermissionEntry> entries, string accountId, ScopePath currentScope)
    {
        _Entries = entries;

        AccountId = accountId;
        CurrentScope = currentScope;
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public Task SetAsync(string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Assertions.NotEmptyOrWhitespace(key);

        var found = ScopedView.FirstOrDefault(c => c.Entry.Key == key);
        if (found != null)
            _Entries.Remove(found);

        _Entries.Add(new AccountPermissionEntry(AccountId, new PermissionEntry(key, value, CurrentScope, metadata)));
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        var removeValues = ScopedView.ToArray();
        foreach(var value in removeValues)
            _Entries.Remove(value);

        return Task.CompletedTask;
    }

    public Task<bool> ContainsAsync(string key)
    {
        var result = ScopedView.Any(k => k.Entry.Key == key);
        return Task.FromResult(result);
    }

    public Task<PermissionEntry> GetAsync(string key)
    {
        var found = ScopedView.FirstOrDefault(c => c.Entry.Key == key);
        if (found == null)
            throw new KeyNotFoundException($"Permission with key '{key}' not found.");

        return Task.FromResult(found.Entry);
    }

    public IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var view = ScopedView.Select(c => c.Entry).ToList().GetEnumerator();
        return new SynchronousAsyncEnumerator<PermissionEntry>(view);
    }

    public Task RemoveAsync(string key)
    {
        var found = ScopedView.FirstOrDefault(c => c.Entry.Key == key);
        if (found != null)
            _Entries.Remove(found);

        return Task.CompletedTask;
    }

    private IEnumerable<AccountPermissionEntry> ScopedView => _Entries.Where(e => e.AccountId == AccountId && e.Entry.Scope == CurrentScope);
}