using Sparcpoint.Extensions.Permissions.Services.InMemory;

namespace Sparcpoint.Extensions.Objects;

internal class InMemoryObjectIdNameQuery : IObjectIdNameQuery
{
    private readonly ObjectEntries _Entries;
    private readonly LockObject _LockObject;

    public InMemoryObjectIdNameQuery(ObjectEntries entries, LockObject lockObject)
    {
        _Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    public IAsyncEnumerable<SparcpointObjectId> RunAsync(ObjectQueryParameters parameters)
    {
        var results = _Entries.PerformQuery(parameters, _LockObject).Select(e => new SparcpointObjectId(e.Id, e.Name)).ToArray();
        return new SynchronousAsyncEnumerable<SparcpointObjectId>(results);
    }
}
