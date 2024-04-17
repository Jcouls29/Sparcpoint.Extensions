namespace Sparcpoint.Extensions.Resources;

public interface ISparcpointClient
{
    Task<ISubscriptionClient> CreateNewSubscriptionAsync(SubscriptionData data);
    IAsyncEnumerable<ISubscriptionClient> GetSubscriptionsAsync();
}
