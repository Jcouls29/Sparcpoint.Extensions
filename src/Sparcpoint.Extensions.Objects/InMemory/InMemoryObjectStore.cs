using System.Collections.Concurrent;

namespace Sparcpoint.Extensions.Objects;

internal class InMemoryObjectStore<T> : IObjectStore<T> where T : ISparcpointObject
{
    private readonly ObjectEntries _Entries;
    private readonly LockObject _LockObject;

    public InMemoryObjectStore(ObjectEntries entries, LockObject lockObject)
    {
        _Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        _LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    public Task DeleteAsync(IEnumerable<ScopePath> ids)
    {
        lock (_LockObject)
        {
            var idSet = new HashSet<ScopePath>(ids);
            var found = _Entries.Where(x => idSet.Contains(x.Id)).ToArray();
            foreach (var e in found)
            {
                _Entries.Remove(e);
            }
        }

        return Task.CompletedTask;
    }

    public Task<T?> FindAsync(ScopePath id)
    {
        lock (_LockObject)
        {
            var found = _Entries.FirstOrDefault(x => x.Id == id);
            if (found == null || found.GetType() != typeof(T))
                throw new KeyNotFoundException($"Object with id '{id}' not found or of the wrong type.");

            return Task.FromResult((T?)found);
        }
    }

    public Task UpsertAsync(IEnumerable<T> o)
    {
        lock (_LockObject)
        {
            var objList = o.ToList();
            var idSet = new HashSet<ScopePath>(objList.Select(i => i.Id));
            var found = _Entries.Where(x => idSet.Contains(x.Id)).ToArray();
            foreach (var e in found)
            {
                _Entries.Remove(e);
            }
            _Entries.AddRange(objList.Cast<ISparcpointObject>());
        }

        return Task.CompletedTask;
    }
}