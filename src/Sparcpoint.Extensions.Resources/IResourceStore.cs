using Sparcpoint.Extensions.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Resources;

public interface IResourceStore
{
    Task Set<T>(T data, params ScopePath[] resourceIds) where T : SparcpointResource;
    Task<T?> Get<T>(ScopePath resourceId) where T : SparcpointResource;
    Task Delete(params ScopePath[] resourceIds);

    Task<IEnumerable<SparcpointResourceEntry>> GetChildEntries(ScopePath parentResourceId, int maxDepth = 2, string[]? includeTypes = null);
}

public static class ResourceStoreExtensions
{
    public static async Task<IEnumerable<T>> GetChildEntries<T>(this IResourceStore store, ScopePath parentResourceId, int maxDepth = 2) where T : SparcpointResource
    {
        var type = ResourceTypeAttribute.GetResourceType(typeof(T));
        var children = await store.GetChildEntries(parentResourceId, maxDepth, includeTypes: new[] { type });

        List<T> results = new();
        foreach(var entry in children)
        {
            var resource = await store.Get<T>(entry.ResourceId);
            if (resource != null)
                results.Add(resource);
        }

        return results;
    }
}
