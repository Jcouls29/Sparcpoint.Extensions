using Sparcpoint.Extensions.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Resources;

public interface IResourceStore
{
    Task SetAsync<T>(T data) where T : SparcpointResource;
    Task<T?> GetAsync<T>(ScopePath resourceId) where T : SparcpointResource;
    Task DeleteAsync(params ScopePath[] resourceIds);

    Task<IEnumerable<SparcpointResourceEntry>> GetChildEntriesAsync(ScopePath parentResourceId, int maxDepth = 2, string[]? includeTypes = null);
}

public interface ISparcpointClient
{
    Task<ISubscriptionClient> CreateNewSubscriptionAsync(SubscriptionData data);
    IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync();
}

public interface ISubscriptionClient : IResourceDataClient<SubscriptionData>
{
    string SubscriptionName { get; }

    Task<IOrganizationClient> CreateNewOrganizationAsync(OrganizationData data);
    IAsyncEnumerable<IOrganizationClient> GetOrganizationsAsync();
}

public interface IOrganizationClient : IResourceDataClient<OrganizationData>
{
    string SubscriptionName { get; }
    string OrganizationName { get; }
}

internal class DefaultSparcpointClient : ISparcpointClient
{
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _Factory;
    private readonly string _AccountId;

    public DefaultSparcpointClient(IResourceStore store, IResourceDataClientFactory factory, string accountId)
    {
        Ensure.ArgumentNotNull(store);
        Ensure.ArgumentNotNull(factory);
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        _Store = store;
        _Factory = factory;
        _AccountId = accountId;
    }

    public async Task<ISubscriptionClient> CreateNewSubscriptionAsync(SubscriptionData data)
    {
        Ensure.ArgumentNotNull(data);
        Ensure.NotNullOrWhiteSpace(data.DisplayName);

        var resourceId = ScopePath.Parse("/subscriptions/" + Guid.NewGuid().ToString());
        var resource = new SparcpointResource<SubscriptionData>
        {
            ResourceId = resourceId,
            Data = data with { OwnerAccountId = _AccountId },
            Permissions = ResourcePermissions.WithOwnerPermissions(_AccountId)
        };

        await _Store.SetAsync(resource);
        await _Store.AddToIndexAsync(_AccountId, resource);

        return new DefaultSubscriptionClient(_Factory.Create<SubscriptionData>(resourceId));
    }

    public async IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync()
    {
        var index = await _Store.GetAccountIndexByResourceType<SparcpointResource<SubscriptionData>>(_AccountId);
        foreach (var entry in index)
        {
            yield return new DefaultSubscriptionClient(_Factory.Create<SubscriptionData>(entry.ResourceId));
        }
    }
}

internal class DefaultSubscriptionClient : ISubscriptionClient
{
    private readonly IResourceDataClient<SubscriptionData> _Client;

    public DefaultSubscriptionClient(IResourceDataClient<SubscriptionData> client)
    {
        _Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public string SubscriptionName => _Client.ResourceId.Segments.Last();
    public ScopePath ResourceId => _Client.ResourceId;

    public async Task DeleteAsync()
        => await _Client.DeleteAsync();

    public async Task<SubscriptionData?> GetAsync()
        => await _Client.GetAsync();

    public async Task<ResourcePermissions?> GetPermissionsAsync()
        => await _Client.GetPermissionsAsync();

    public async Task SaveAsync(SubscriptionData data)
        => await _Client.SaveAsync(data);

    public async Task SetPermissionsAsync(ResourcePermissions permissions)
        => await _Client.SetPermissionsAsync(permissions);

    public Task<IOrganizationClient> CreateNewOrganizationAsync(OrganizationData data)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<IOrganizationClient> GetOrganizationsAsync()
    {
        throw new NotImplementedException();
    }
}