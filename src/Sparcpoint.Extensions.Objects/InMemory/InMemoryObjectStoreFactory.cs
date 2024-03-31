namespace Sparcpoint.Extensions.Objects;

internal class InMemoryObjectStoreFactory : IObjectStoreFactory
{
    private readonly ObjectEntries _Entries;
    private readonly LockObject _LockObject;

    public InMemoryObjectStoreFactory(ObjectEntries entries, LockObject lockObject)
    {
        _Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    public IObjectQuery<T> CreateQuery<T>() where T : class, ISparcpointObject
        => new InMemoryObjectQuery<T>(_Entries, _LockObject);

    public IObjectStore<T> CreateStore<T>() where T : class, ISparcpointObject
        => new InMemoryObjectStore<T>(_Entries, _LockObject);
}
