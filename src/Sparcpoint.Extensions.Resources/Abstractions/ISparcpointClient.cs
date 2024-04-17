namespace Sparcpoint.Extensions.Resources;

public interface ISparcpointClient
{
    Task<ISubscriptionClient> CreateNewSubscriptionAsync(SubscriptionData data);
    IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync();

    IResourceDataClient<T> GetResourceClient<T>(ScopePath resourceId) where T : class, new();
}
