namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Organization")]
public sealed class OrganizationResource : SparcpointResource
{
    public const string RESOURCE_FORMAT = "/subscriptions/{0}/organizatons/{1}";

    public string DisplayName { get; set; }
}

public static class Organization
{
    // TODO: Create a new organization...
}