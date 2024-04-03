using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionView : IScopePermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly bool _IncludeRootScope;
    private readonly string[]? _Keys;
    private readonly string[]? _AccountIds;

    public InMemoryScopePermissionView(List<AccountPermissionEntry> entries, ScopePath currentScope, bool includeRootScope, string[]? keys, string[]? accountIds)
    {
        _Entries = entries;
        CurrentScope = currentScope;
        _IncludeRootScope = includeRootScope;
        _Keys = keys;
        _AccountIds = accountIds;
    }

    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var entries = _Entries.CalculateView(CurrentScope, _IncludeRootScope, _Keys, _AccountIds);
        return new SynchronousAsyncEnumerator<AccountPermissionEntry>(entries.GetEnumerator());
    }
}