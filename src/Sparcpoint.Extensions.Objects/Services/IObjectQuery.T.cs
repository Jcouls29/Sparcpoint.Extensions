using Sparcpoint.Extensions.Objects;

public interface IObjectQuery<T> where T : ISparcpointObject
{
    IAsyncEnumerable<T> RunAsync(ObjectQueryParameters parameters);
}