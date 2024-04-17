namespace Sparcpoint.Extensions.Resources;

internal class DefaultResourceDataClientFactory : IResourceDataClientFactory
{
    private readonly IResourceStore _Store;

    public DefaultResourceDataClientFactory(IResourceStore store)
    {
        Ensure.ArgumentNotNull(store);
        
        _Store = store;
    }

    public IResourceDataClient<T> Create<T>(ScopePath resourceId) where T : class, new()
    {
        return new DefaultResourceDataClient<T>(_Store, this, resourceId);
    }
}
