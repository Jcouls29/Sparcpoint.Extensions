
namespace Sparcpoint.Extensions.Permissions.Services.InMemory;

public class SynchronousAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _Values;

    public SynchronousAsyncEnumerator(IEnumerator<T> values)
    {
        _Values = values;
    }

    public T Current => _Values.Current;

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        var result = _Values.MoveNext();
        return ValueTask.FromResult(result);
    }
}