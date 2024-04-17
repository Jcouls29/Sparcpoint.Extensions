
namespace Sparcpoint.Extensions.Resources;

internal class DefaultResourceDataClient<T> : IResourceDataClient<T> where T : class, new()
{
    private readonly IResourceStore _Store;
    private readonly IResourceDataClientFactory _Factory;

    public DefaultResourceDataClient(IResourceStore store, IResourceDataClientFactory factory, ScopePath resourceId)
    {
        Ensure.ArgumentNotNull(_Store);
        Ensure.ArgumentNotNull(factory);
        Ensure.NotEqual(ScopePath.RootScope, resourceId);

        _Store = store;
        _Factory = factory;
        ResourceId = resourceId;
    }

    public ScopePath ResourceId { get; }

    public async Task DeleteAsync()
    {
        await _Store.DeleteAsync(ResourceId);
    }

    public async Task<T?> GetAsync()
    {
        var found = await _Store.GetAsync<SparcpointResource<T>>(ResourceId);
        return found?.Data;
    }

    public IResourceDataClient<TChild> GetChildClient<TChild>(ScopePath relativePath) where TChild : class, new()
    {
        var resourceId = ResourceId + relativePath;
        return _Factory.Create<TChild>(resourceId);
    }

    public async IAsyncEnumerable<IResourceDataClient<TChild>> GetChildClientsAsync<TChild>(int maxDepth = 2) where TChild : class, new()
    {
        var resourceType = ResourceTypeAttribute.GetResourceType<TChild>();
        var found = _Store.GetChildEntriesAsync(ResourceId, maxDepth, includeTypes: new[] { resourceType });
        await foreach (var item in found)
            yield return _Factory.Create<TChild>(item.ResourceId);
    }

    public async Task<ResourcePermissions> GetPermissionsAsync()
    {
        var found = await _Store.GetAsync<SparcpointResource<T>>(ResourceId);
        return found?.Permissions ?? new();
    }

    public async Task SaveAsync(T data)
    {
        var found = await _Store.GetAsync<SparcpointResource<T>>(ResourceId);
        if (found == null)
        {
            found = new SparcpointResource<T>();
            found.ResourceId = ResourceId;
        }

        found.Data = data;

        await _Store.SetAsync(found);
    }

    public async Task SetPermissionsAsync(ResourcePermissions permissions)
    {
        var found = await _Store.GetAsync<SparcpointResource<T>>(ResourceId);
        if (found == null)
            throw new InvalidOperationException($"Could not find resource: {ResourceId}");

        found.Permissions = permissions;
        await _Store.SetAsync(found);
    }
}