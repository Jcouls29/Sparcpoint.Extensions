namespace Sparcpoint.Extensions.Resources;

public interface ISubscriptionClient : IResourceDataClient<SubscriptionData>
{
    string SubscriptionName { get; }

    Task<IOrganizationClient> CreateNewOrganizationAsync(OrganizationData data);
    IAsyncEnumerable<IOrganizationClient> GetOrganizationsAsync();
}
