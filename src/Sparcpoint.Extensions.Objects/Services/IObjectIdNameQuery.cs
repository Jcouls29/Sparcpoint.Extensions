namespace Sparcpoint.Extensions.Objects;

public interface IObjectIdNameQuery
{
    IAsyncEnumerable<SparcpointObjectId> RunAsync(ObjectQueryParameters parameters);
}
