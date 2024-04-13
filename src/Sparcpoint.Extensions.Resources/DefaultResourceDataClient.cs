namespace Sparcpoint.Extensions.Resources;

internal class DefaultResourceDataClient<T> : IResourceDataClient<T> where T : class, new()
{
    private readonly IResourceStore _Store;

    public DefaultResourceDataClient(IResourceStore store, ScopePath resourceId)
    {
        _Store = store ?? throw new ArgumentNullException(nameof(store));
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

    public async Task<ResourcePermissions?> GetPermissionsAsync()
    {
        var found = await _Store.GetAsync<SparcpointResource<T>>(ResourceId);
        return found?.Permissions;
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