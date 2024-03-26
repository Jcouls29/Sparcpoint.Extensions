
namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

internal class SynchronousAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IEnumerable<T> _Values;

    public SynchronousAsyncEnumerable(IEnumerable<T> values)
    {
        _Values = values;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new SynchronousAsyncEnumerator<T>(_Values.GetEnumerator());
    }
}
