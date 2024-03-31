namespace Sparcpoint.Extensions.Objects;

public sealed class FactoryObjectQueryWrapper<T> : IObjectQuery<T> where T : class, ISparcpointObject
{
    private readonly IObjectQuery<T> _InnerService;

    public FactoryObjectQueryWrapper(IObjectStoreFactory factory)
    {
        _InnerService = factory.CreateQuery<T>();
    }

    public IAsyncEnumerable<T> RunAsync(ObjectQueryParameters parameters)
        => _InnerService.RunAsync(parameters);
}