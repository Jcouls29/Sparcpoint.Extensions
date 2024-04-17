namespace Sparcpoint.Extensions.Resources;

public class SparcpointClientFactory : ISparcpointClientFactory
{
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _ClientFactory;

    public SparcpointClientFactory(IResourceStore store, IResourceDataClientFactory clientFactory)
    {
        Ensure.ArgumentNotNull(store);
        Ensure.ArgumentNotNull(clientFactory);

        _Store = store;
        _ClientFactory = clientFactory;
    }

    public SparcpointClientFactory(IResourceStore store) : this(store, new DefaultResourceDataClientFactory(store)) { }

    public ISparcpointClient Create(string accountId)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        return new SparcpointClient(_Store, _ClientFactory, accountId);
    }
}
