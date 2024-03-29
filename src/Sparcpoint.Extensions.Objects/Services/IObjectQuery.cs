namespace Sparcpoint.Extensions.Objects;

public interface IObjectQuery
{
    IAsyncEnumerable<ISparcpointObject> RunAsync(ObjectQueryParameters parameters);
}
