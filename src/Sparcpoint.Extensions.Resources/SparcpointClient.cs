﻿namespace Sparcpoint.Extensions.Resources;

public class SparcpointClient : ISparcpointClient
{
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _Factory;
    private readonly string _AccountId;

    public SparcpointClient(IResourceStore store, IResourceDataClientFactory factory, string accountId)
    {
        Ensure.ArgumentNotNull(store);
        Ensure.ArgumentNotNull(factory);
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        _Store = store;
        _Factory = factory;
        _AccountId = accountId;
    }

    public SparcpointClient(IResourceStore store, string accountId) : this(store, new DefaultResourceDataClientFactory(store), accountId) { }

    public async Task<ISubscriptionClient> CreateNewSubscriptionAsync(SubscriptionData data)
    {
        Ensure.ArgumentNotNull(data);
        Ensure.NotNullOrWhiteSpace(data.DisplayName);

        var resourceId = SubscriptionData.CreateResourceId(Guid.NewGuid().ToString());
        var resource = new SparcpointResource<SubscriptionData>
        {
            ResourceId = resourceId,
            Data = data with { OwnerAccountId = _AccountId },
            Permissions = ResourcePermissions.WithOwnerPermissions(_AccountId)
        };

        await _Store.SetAsync(resource);
        await _Store.AddToIndexAsync(_AccountId, resource);

        return new DefaultSubscriptionClient(_Store, _Factory, _Factory.Create<SubscriptionData>(resourceId), _AccountId);
    }

    public async IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync()
    {
        var index = await _Store.GetAccountIndexByResourceType<SparcpointResource<SubscriptionData>>(_AccountId);
        foreach (var entry in index)
        {
            yield return new DefaultSubscriptionClient(_Store, _Factory, _Factory.Create<SubscriptionData>(entry.ResourceId), _AccountId);
        }
    }
}
