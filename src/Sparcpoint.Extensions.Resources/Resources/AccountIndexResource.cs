namespace Sparcpoint.Extensions.Resources;

[ResourceType("Account.Index")]
public sealed class AccountIndexResource : SparcpointResource
{
    public const string RESOURCE_FORMAT = "/accounts/{0}";

    public List<AccountIndexEntry> Resources { get; set; } = new();
}

public class AccountIndexEntry
{
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }
}

public static class AccountIndex
{
    public static async Task<AccountIndexResource> GetAccountIndexAsync(this IResourceStore store, string accountId)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        ScopePath resourceId = string.Format(AccountIndexResource.RESOURCE_FORMAT, accountId);
        var found = await store.GetAsync<AccountIndexResource>(resourceId);
        if (found == null)
        {
            found = new AccountIndexResource
            {
                ResourceId = resourceId,
                Permissions = ResourcePermissions.With(accountId, b => b.CanReadData().CanWriteData()),
            };
            await store.SetAsync(found);
        }

        return found;
    }

    public static async Task<IEnumerable<AccountIndexEntry>> GetAccountIndexByResourceType(this IResourceStore store, string accountId, string resourceType)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(resourceType);

        var index = await store.GetAccountIndexAsync(accountId);
        return index.Resources.Where(r => r.ResourceType == resourceType).ToArray();
    }

    public static async Task<IEnumerable<AccountIndexEntry>> GetAccountIndexByResourceType<T>(this IResourceStore store, string accountId) where T : SparcpointResource
    {
        var resourceType = ResourceTypeAttribute.GetResourceType(typeof(T));
        return await GetAccountIndexByResourceType(store, accountId, resourceType);
    }

    public static async Task AddToIndexAsync(this IResourceStore store, string accountId, params AccountIndexEntry[] entries)
    {
        var index = await store.GetAccountIndexAsync(accountId);

        foreach (var entry in entries)
        {
            var isFound = index.Resources.Any(r => r.ResourceType == entry.ResourceType && r.ResourceId == entry.ResourceId);
            if (!isFound)
            {
                index.Resources.Add(new AccountIndexEntry { ResourceId = entry.ResourceId, ResourceType = entry.ResourceType });
            }
        }

        await store.SetAsync(index);
    }

    public static async Task AddToIndexAsync(this IResourceStore store, string accountId, string resourceType, ScopePath resourceId)
        => await AddToIndexAsync(store, accountId, new AccountIndexEntry { ResourceId = resourceId, ResourceType = resourceType });

    public static async Task AddToIndexAsync<T>(this IResourceStore store, string accountId, T resource) where T : SparcpointResource
        => await AddToIndexAsync(store, accountId, resource.ResourceType, resource.ResourceId);

    public static async Task RemoveFromIndexAsync(this IResourceStore store, string accountId, params AccountIndexEntry[] entries)
    {
        var index = await store.GetAccountIndexAsync(accountId);

        foreach (var entry in entries)
        {
            var found = index.Resources.FirstOrDefault(r => r.ResourceType == entry.ResourceType && r.ResourceId == entry.ResourceId);
            if (found != null)
                index.Resources.Remove(found);
        }

        await store.SetAsync(index);
    }

    public static async Task RemoveFromIndexAsync(this IResourceStore store, string accountId, string resourceType, ScopePath resourceId)
        => await RemoveFromIndexAsync(store, accountId, new AccountIndexEntry { ResourceId = resourceId, ResourceType = resourceType });

    public static async Task RemoveFromIndexAsync<T>(this IResourceStore store, string accountId, T resource) where T : SparcpointResource
        => await RemoveFromIndexAsync(store, accountId, resource.ResourceType, resource.ResourceId);
}