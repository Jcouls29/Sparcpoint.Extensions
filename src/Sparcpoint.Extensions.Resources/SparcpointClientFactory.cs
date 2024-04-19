using LazyCache;

namespace Sparcpoint.Extensions.Resources;

public class SparcpointClientFactory : ISparcpointClientFactory
{
    private readonly IResourceStore _Store;
    private readonly IPermissionCache _PermissionCache;

    public SparcpointClientFactory(IResourceStore store)
    {
        Ensure.ArgumentNotNull(store);
        
        _PermissionCache = new DefaultPermissionsCache(store, new CachingService());
        _Store = store;
    }

    public ISparcpointClient Create(string accountId)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        var protectedStore = new ProtectedResourceStore(_Store, _PermissionCache, accountId);
        var clientFactory = new DefaultResourceDataClientFactory(protectedStore);

        return new SparcpointClient(protectedStore, clientFactory, accountId);
    }
}
