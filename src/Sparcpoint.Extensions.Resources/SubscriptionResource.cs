namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Subscription")]
public sealed class SubscriptionResource : SparcpointResource
{
    public const string RESOURCE_FORMAT = "/subscriptions/{0}";

    public string OwnerAccountId { get; set; }
    public string DisplayName { get; set; }
}

public static class Subscription
{
    public static async Task<SubscriptionResource> CreateNewSubscriptionAsync(this IResourceStore store, string accountId, string displayName)
    {
        // TODO: More flexible, predictable way to generate the subscription name
        ScopePath resourceId = string.Format(SubscriptionResource.RESOURCE_FORMAT, Guid.NewGuid().ToString());
        var resource = new SubscriptionResource
        {
            ResourceId = resourceId,
            DisplayName = displayName,
            OwnerAccountId = accountId,
            Permissions = ResourcePermissions.WithOwnerPermissions(accountId)
        };

        await store.SetAsync(resource);
        await store.AddToIndexAsync(accountId, resource);
        return resource;
    }

    public static async IAsyncEnumerable<SubscriptionResource> GetSubscriptionsAsync(this IResourceStore store, string accountId)
    {
        var availableSubscriptions = await store.GetAccountIndexByResourceType(accountId, ResourceTypeAttribute.GetResourceType(typeof(SubscriptionResource)));
        foreach(var sub in availableSubscriptions)
        {
            var found = await store.GetAsync<SubscriptionResource>(sub.ResourceId);
            if (found != null)
                yield return found;
        }
    }
}