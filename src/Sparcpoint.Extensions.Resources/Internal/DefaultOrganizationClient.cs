

namespace Sparcpoint.Extensions.Resources;

internal class DefaultOrganizationClient : IOrganizationClient
{
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _Factory;
    private readonly IResourceDataClient<OrganizationData> _Client;
    private readonly string _AccountId;

    public DefaultOrganizationClient(IResourceStore store, IResourceDataClientFactory factory, IResourceDataClient<OrganizationData> client, string accountId)
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

    public string SubscriptionName => _Client.ResourceId.Segments[1];
    public string OrganizationName => _Client.ResourceId.Segments[3];
    public ScopePath ResourceId => _Client.ResourceId;

    public async Task DeleteAsync()
        => await _Client.DeleteAsync();

    public async Task<OrganizationData?> GetAsync()
        => await _Client.GetAsync();

    public IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(int maxDepth = 2) where TChild : class, new()
        => _Client.GetChildClientsAsync<TChild>(maxDepth);

    public IResourceDataClient<TChild> GetChildClient<TChild>(ScopePath relativePath) where TChild : class, new()
        => _Client.GetChildClient<TChild>(relativePath);

    public async Task<ResourcePermissions?> GetPermissionsAsync()
        => await _Client.GetPermissionsAsync();

    public async Task SaveAsync(OrganizationData data)
        => await _Client.SaveAsync(data);

    public async Task SetPermissionsAsync(ResourcePermissions permissions)
        => await _Client.SetPermissionsAsync(permissions);
}
