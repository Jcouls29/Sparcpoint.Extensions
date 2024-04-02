using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryPermissionStore : IPermissionStore, IAccountPermissionQuery
{
    private readonly List<AccountPermissionEntry> _Entries;
    private object _LockObject;

    public InMemoryPermissionStore(List<AccountPermissionEntry> entries)
    {
        _Entries = entries;
        _LockObject = new();

        Permissions = new InMemoryScopePermissionCollection(_Entries, _LockObject);
    }

    public IScopePermissionCollection Permissions { get; }

    public IAccountPermissionView GetView(string accountId, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        return new InMemoryAccountPermissionView(_Entries, _LockObject, accountId, scope ?? ScopePath.RootScope);
    }

    public IScopePermissionView GetView(ScopePath scope)
    {
        return new InMemoryScopePermissionView(_Entries, _LockObject, scope);
    }

    public async IAsyncEnumerable<PermissionEntry> RunAsync(string accountId, PermissionQueryParameters parameters)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        IQueryable<AccountPermissionEntry> query = _Entries.AsQueryable();
        query = query.Where(e => e.AccountId == accountId);

        if (!string.IsNullOrWhiteSpace(parameters.KeyExact))
            query = query.Where(e => e.Entry.Key == parameters.KeyExact);
        if (!string.IsNullOrWhiteSpace(parameters.KeyStartsWith))
            query = query.Where(e => e.Entry.Key.StartsWith(parameters.KeyStartsWith));
        if (!string.IsNullOrWhiteSpace(parameters.KeyEndsWith))
            query = query.Where(e => e.Entry.Key.EndsWith(parameters.KeyEndsWith));
        if (parameters.ScopeExact != null)
            query = query.Where(e => e.Scope == parameters.ScopeExact);
        if (parameters.ScopeStartsWith != null)
            query = query.Where(e => parameters.ScopeStartsWith < e.Scope);
        if (parameters.ValueExact != null)
            query = query.Where(e => e.Entry.Value == parameters.ValueExact);
        if (parameters.ScopeStartsWith != null && parameters.ImmediateChildrenOnly)
            query = query.Where(e => e.Scope.Rank == parameters.ScopeStartsWith.Value.Rank + 1);

        if (parameters.ScopeEndsWith != null)
        {
            var endsWithValue = parameters.ScopeEndsWith.Value.ToString();
            query = query.Where(e => e.Scope.ToString().EndsWith(endsWithValue));
        }

        if (parameters.WithMetadata != null && parameters.WithMetadata.Count > 0)
        {
            query = query.Where(e => e.Entry.Metadata != null);
            foreach (var entry in parameters.WithMetadata)
            {
                query = query.Where(e => e.Entry.Metadata.ContainsKey(entry.Key) && e.Entry.Metadata[entry.Key] == entry.Value);
            }
        }

        var result = query.Select(e => e.Entry).ToArray();
        foreach(var entry in result)
        {
            yield return entry;
        }
    }

    public Task<bool> HasAccessAsync(string accountId, string key, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var view = _Entries.Where(e => e.AccountId == accountId && e.Entry.Key == key);
        var result = view.CalculatePermissionValue(scope ?? ScopePath.RootScope, accountId, key);

        return Task.FromResult(result == PermissionValue.Allow);
    }
}
