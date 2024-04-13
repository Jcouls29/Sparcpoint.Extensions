namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Organization")]
public sealed class OrganizationResource : SparcpointResource
{
    public const string RESOURCE_FORMAT = "/subscriptions/{0}/organizatons/{1}";

    public string DisplayName { get; set; }
}

public static class Organization
{
    public static async Task<OrganizationResource> CreateNewOrganizationAsync(this IResourceStore store, string accountId, string subscriptionName, string displayName)
    {
        // TODO: More flexible, predicatable way to generate the subscription name
        ScopePath resourceId = string.Format(OrganizationResource.RESOURCE_FORMAT, subscriptionName, Guid.NewGuid().ToString());
    }
}