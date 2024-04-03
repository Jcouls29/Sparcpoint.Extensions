using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionView : IScopePermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly bool _IncludeRootScope;

    public InMemoryScopePermissionView(List<AccountPermissionEntry> entries, ScopePath currentScope, bool includeRootScope)
    {
        _Entries = entries;
        CurrentScope = currentScope;
        _IncludeRootScope = includeRootScope;
    }

    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var entries = _Entries.CalculateView(CurrentScope, _IncludeRootScope);
        return new SynchronousAsyncEnumerator<AccountPermissionEntry>(entries.GetEnumerator());
    }
}