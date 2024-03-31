using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class InMemoryScopePermissionView : IScopePermissionView
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly object _LockObject;

    public InMemoryScopePermissionView(List<AccountPermissionEntry> entries, object lockObject, ScopePath currentScope)
    {
        _Entries = entries;
        _LockObject = lockObject;

        CurrentScope = currentScope;
    }

    public ScopePath CurrentScope { get; }

    public IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var entries = _Entries.CalculateView(CurrentScope);
        return new SynchronousAsyncEnumerator<AccountPermissionEntry>(entries.GetEnumerator());
    }
}