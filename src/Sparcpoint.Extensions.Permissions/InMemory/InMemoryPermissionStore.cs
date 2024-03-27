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

    public InMemoryPermissionStore(List<AccountPermissionEntry> entries)
    {
        _Entries = entries;
    }

    public IAccountPermissionCollection Get(string accountId, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);

        return new InMemoryAccountPermissionCollection(_Entries, accountId, scope ?? ScopePath.RootScope);
    }

    public IAccountPermissionView GetView(string accountId, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);

        return new InMemoryAccountPermissionView(_Entries, accountId, scope ?? ScopePath.RootScope);
    }

    public IScopePermissionCollection Get(ScopePath scope)
    {
        return new InMemoryScopePermissionCollection(_Entries, scope);
    }

    public IScopePermissionView GetView(ScopePath scope)
    {
        return new InMemoryScopePermissionView(_Entries, scope);
    }

    public Task<IEnumerable<PermissionEntry>> RunAsync(string accountId, PermissionQueryParameters parameters)
    {
        Assertions.NotEmptyOrWhitespace(accountId);

        IQueryable<AccountPermissionEntry> query = _Entries.AsQueryable();
        query = query.Where(e => e.AccountId == accountId);

        if (!string.IsNullOrWhiteSpace(parameters.KeyExact))
            query = query.Where(e => e.Entry.Key == parameters.KeyExact);
        if (!string.IsNullOrWhiteSpace(parameters.KeyStartsWith))
            query = query.Where(e => e.Entry.Key.StartsWith(parameters.KeyStartsWith));
        if (!string.IsNullOrWhiteSpace(parameters.KeyEndsWith))
            query = query.Where(e => e.Entry.Key.EndsWith(parameters.KeyEndsWith));
        if (parameters.ScopeExact != null)
            query = query.Where(e => e.Entry.Scope == parameters.ScopeExact);
        if (parameters.ScopeStartsWith != null)
            query = query.Where(e => parameters.ScopeStartsWith < e.Entry.Scope);
        if (parameters.ValueExact != null)
            query = query.Where(e => e.Entry.Value == parameters.ValueExact);
        if (parameters.ScopeExact != null && parameters.ImmediateChildrenOnly)
            query = query.Where(e => e.Entry.Scope.Rank == parameters.ScopeExact.Value.Rank + 1);

        if (parameters.ScopeEndsWith != null)
        {
            var endsWithValue = parameters.ScopeEndsWith.Value.ToString();
            query = query.Where(e => e.Entry.Scope.ToString().EndsWith(endsWithValue));
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
        return Task.FromResult((IEnumerable<PermissionEntry>)result);
    }

    public Task<bool> HasAccessAsync(string accountId, string key, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(key);

        var view = _Entries.Where(e => e.AccountId == accountId && e.Entry.Key == key);
        var result = view.CalculatePermissionValue(scope ?? ScopePath.RootScope, accountId, key);

        return Task.FromResult(result == PermissionValue.Allow);
    }
}
