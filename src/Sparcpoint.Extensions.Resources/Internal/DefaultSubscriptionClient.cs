using Slugify;

namespace Sparcpoint.Extensions.Resources;

internal class DefaultSubscriptionClient : ISubscriptionClient
{
    private static SlugHelper _Slugs = new();
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _Factory;
    private readonly IResourceDataClient<SubscriptionData> _Client;
    private readonly string _AccountId;

    public DefaultSubscriptionClient(IResourceStore store, IResourceDataClientFactory factory, IResourceDataClient<SubscriptionData> client, string accountId)
    {
        Ensure.ArgumentNotNull(store);
        Ensure.ArgumentNotNull(factory);
        Ensure.ArgumentNotNull(client);
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        _Store = store;
        _Factory = factory;
        _Client = client;
        _AccountId = accountId;
    }

    public string SubscriptionName => _Client.ResourceId.Segments.Last();
    public ScopePath ResourceId => _Client.ResourceId;

    public async Task DeleteAsync()
        => await _Client.DeleteAsync();

    public async Task<SubscriptionData?> GetAsync()
        => await _Client.GetAsync();

    public async Task<ResourcePermissions> GetPermissionsAsync()
        => await _Client.GetPermissionsAsync();

    public async Task SaveAsync(SubscriptionData data)
        => await _Client.SaveAsync(data);

    public async Task SetPermissionsAsync(ResourcePermissions permissions)
        => await _Client.SetPermissionsAsync(permissions);

    public IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(int maxDepth = 2) where TChild : class, new()
        => _Client.GetChildClientsAsync<TChild>(maxDepth);

    public IResourceDataClient<TChild> GetChildClient<TChild>(ScopePath relativePath) where TChild : class, new()
        => _Client.GetChildClient<TChild>(relativePath);

    public async Task<IOrganizationClient> CreateNewOrganizationAsync(OrganizationData data)
    {
        Ensure.ArgumentNotNull(data);
        Ensure.NotNullOrWhiteSpace(data.DisplayName);

        var resourceId = OrganizationData.CreateResourceId(SubscriptionName, data.DisplayName.Slugify());
        if (await _Store.ExistsAsync(resourceId))
            throw new InvalidOperationException($"Organization with display name '{data.DisplayName}' already exists.");

        var resource = new SparcpointResource<OrganizationData>
        {
            ResourceId = resourceId,
            Data = data,
            Permissions = ResourcePermissions.WithOwnerPermissions(_AccountId)
        };

        await _Store.SetAsync(resource);
        await _Store.AddToIndexAsync(_AccountId, resource);

        return new DefaultOrganizationClient(_Store, _Factory, _Factory.Create<OrganizationData>(resourceId), _AccountId);
    }

    public async IAsyncEnumerable<IOrganizationClient> GetOrganizationsAsync()
    {
        var index = await _Store.GetAccountIndexByResourceType<SparcpointResource<OrganizationData>>(_AccountId);
        foreach(var entry in index)
        {
            yield return new DefaultOrganizationClient(_Store, _Factory, _Factory.Create<OrganizationData>(entry.ResourceId), _AccountId);
        }
    }
}
