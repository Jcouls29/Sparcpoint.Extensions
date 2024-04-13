namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Organization")]
public sealed class OrganizationData
{
    public const string RESOURCE_FORMAT = "/subscriptions/{0}/organizatons/{1}";

    public string DisplayName { get; set; }
}