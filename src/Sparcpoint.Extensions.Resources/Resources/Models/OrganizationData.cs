namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Organization")]
public sealed class OrganizationData
{
    [ResourceId]
    public ScopePath ResourceId { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    private const string RESOURCE_FORMAT = "/subscriptions/{0}/organizatons/{1}";
    internal static ScopePath CreateResourceId(string subscriptionName, string organizationName)
        => string.Format(RESOURCE_FORMAT, subscriptionName, organizationName);
}