using System.Collections.Concurrent;

namespace Sparcpoint.Extensions.Objects;

public interface IObjectStore<T> where T : ISparcpointObject
{
    Task<T?> FindAsync(ScopePath id);
    Task UpsertAsync(IEnumerable<T> o);
    Task DeleteAsync(IEnumerable<T> o);
}
