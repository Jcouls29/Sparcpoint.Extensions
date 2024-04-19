using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Resources.InMemory;

internal class InMemoryResourceStore : IResourceStore
{
    private readonly InMemoryResourceCollection _Collection;

    public InMemoryResourceStore(InMemoryResourceCollection collection)
    {
        Ensure.ArgumentNotNull(collection);

        _Collection = collection;
    }

    public Task DeleteAsync(params ScopePath[] resourceIds)
    {
        foreach(var id in resourceIds)
        {
            if (_Collection.ContainsKey(id))
                SpinWait.SpinUntil(() => _Collection.TryRemove(id, out _), TimeSpan.FromSeconds(1));
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(ScopePath resourceId, string? resourceType = null)
    {
        if (!_Collection.TryGetValue(resourceId, out var result))
            return Task.FromResult(false);

        if (!string.IsNullOrWhiteSpace(resourceType))
            return Task.FromResult(result.ResourceType == resourceType);

        return Task.FromResult(true);
    }

    public Task<T?> GetAsync<T>(ScopePath resourceId) where T : SparcpointResource
    {
        if (_Collection.TryGetValue(resourceId, out var result) && result.Resource != null && result.Resource is T resource)
            return Task.FromResult((T?)resource);

        return Task.FromResult((T?)null);
    }

    public async IAsyncEnumerable<SparcpointResourceEntry> GetChildEntriesAsync(ScopePath parentResourceId, int maxDepth = int.MaxValue, string[]? includeTypes = null)
    {
        var keys = _Collection.Keys.ToArray();
        var rank = parentResourceId.Rank;
        var children = keys.Where(k => k.StartsWith(parentResourceId) && (k.Rank - rank) <= maxDepth && k.Rank > rank).ToArray();

        bool checkTypes = includeTypes?.Any() ?? false;
        foreach(var item in children)
        {
            if (!_Collection.TryGetValue(item, out var resource))
                throw new InvalidOperationException("Could not retrieve resource.");

            if (checkTypes && !includeTypes!.Any(t => t == resource.ResourceType))
                continue;

            yield return new SparcpointResourceEntry(item, resource.ResourceType, resource.Permissions);
        }
    }

    public async Task<ResourcePermissions?> GetPermissionsAsync(ScopePath resourceId)
    {
        var o = await GetAsync<SparcpointResource>(resourceId);
        if (o == null)
            return null;

        return o.Permissions;
    }

    public Task SetAsync<T>(T data) where T : SparcpointResource
    {
        var resourceId = data.ResourceId;
        var resourceType = data.ResourceType;
        var permissions = data.Permissions;

        _Collection.AddOrUpdate(resourceId, new InMemoryResource
        {
            ResourceId = resourceId,
            Permissions = permissions ?? new(),
            ResourceType = resourceType,
            Resource = data
        }, (id, r) =>
        {
            if (permissions != null)
                r.Permissions = permissions;

            r.Resource = data;

            return r;
        });

        return Task.CompletedTask;
    }

    public Task SetPermissionsAsync(ScopePath resourceId, ResourcePermissions permissions)
    {
        if (!_Collection.TryGetValue(resourceId, out var result))
            throw new InvalidOperationException($"Resource not found. [Resource Id = {resourceId}]");

        result.Permissions = permissions;

        return Task.CompletedTask;
    }
}
