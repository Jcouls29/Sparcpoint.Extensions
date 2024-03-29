using Sparcpoint.Extensions.Permissions.Services.InMemory;

namespace Sparcpoint.Extensions.Objects;

internal class InMemoryObjectQuery<T> : IObjectQuery<T> where T : ISparcpointObject
{
    private readonly ObjectEntries _Entries;
    private readonly LockObject _LockObject;

    public InMemoryObjectQuery(ObjectEntries entries, LockObject lockObject)
    {
        _Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    public IAsyncEnumerable<T> RunAsync(ObjectQueryParameters parameters)
    {
        var results = _Entries.PerformQuery(parameters with { WithType = typeof(T) }, _LockObject).Cast<T>().ToArray();
        return new SynchronousAsyncEnumerable<T>(results);
    }
}
