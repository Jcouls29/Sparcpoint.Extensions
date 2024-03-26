using Sparcpoint.Extensions.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryPermissionStore : IPermissionStore
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

    public Task<bool> IsAllowedAsync(string accountId, string key, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(accountId);
        Assertions.NotEmptyOrWhitespace(key);

        var value = _Entries.CalculatePermissionValue(scope ?? ScopePath.RootScope, accountId, key);
        if (value == PermissionValue.Allow)
            return Task.FromResult(true);

        return Task.FromResult(false);
    }
}
