using Sparcpoint.Extensions.Permissions.Services.InMemory;

namespace Sparcpoint.Extensions.Objects;

internal class InMemoryObjectQuery : IObjectQuery
{
    private readonly ObjectEntries _Entries;
    private readonly LockObject _LockObject;

    public InMemoryObjectQuery(ObjectEntries entries, LockObject lockObject)
    {
        _Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    public IAsyncEnumerable<ISparcpointObject> RunAsync(ObjectQueryParameters parameters)
    {
        var results = _Entries.PerformQuery(parameters, _LockObject);
        return new SynchronousAsyncEnumerable<ISparcpointObject>(results);
    }
}