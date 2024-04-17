namespace Sparcpoint.Extensions.Resources;

public interface IOrganizationClient : IResourceDataClient<OrganizationData>
{
    string SubscriptionName { get; }
    string OrganizationName { get; }
}
