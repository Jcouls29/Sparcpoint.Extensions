namespace Sparcpoint.Extensions.Resources;

public static class ResourceStoreExtensions
{
    public static async Task<IEnumerable<T>> GetChildEntries<T>(this IResourceStore store, ScopePath parentResourceId, int maxDepth = 2) where T : SparcpointResource
    {
        var type = ResourceTypeAttribute.GetResourceType(typeof(T));
        var children = await store.GetChildEntriesAsync(parentResourceId, maxDepth, includeTypes: new[] { type });

        List<T> results = new();
        foreach(var entry in children)
        {
            var resource = await store.GetAsync<T>(entry.ResourceId);
            if (resource != null)
                results.Add(resource);
        }

        return results;
    }
}

