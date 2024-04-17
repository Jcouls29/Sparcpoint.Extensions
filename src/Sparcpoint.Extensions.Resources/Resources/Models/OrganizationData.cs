namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Organization")]
public sealed class OrganizationData
{
    public string DisplayName { get; set; }

    private const string RESOURCE_FORMAT = "/subscriptions/{0}/organizatons/{1}";
    internal static ScopePath CreateResourceId(string subscriptionName, string organizationName)
        => string.Format(RESOURCE_FORMAT, subscriptionName, organizationName);
}