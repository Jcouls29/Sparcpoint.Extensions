using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
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

    public IScopePermissionView GetView(ScopePath scope, bool includeRootScope = false, string[]? keys = null, string[]? accountIds = null)
    {
        return new InMemoryScopePermissionView(_Entries, scope, includeRootScope, keys, accountIds);
    }

    public async IAsyncEnumerable<AccountPermissionEntry> RunAsync(PermissionQueryParameters parameters)
    {
        IQueryable<AccountPermissionEntry> query = _Entries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.AccountId))
            query = query.Where(e => e.AccountId == parameters.AccountId);
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

        var result = query.ToArray();
        foreach(var entry in result)
        {
            yield return entry;
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously