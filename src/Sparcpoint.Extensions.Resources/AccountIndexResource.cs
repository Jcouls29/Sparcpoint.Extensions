namespace Sparcpoint.Extensions.Resources;

[ResourceType("Account.Index")]
public sealed class AccountIndexResource : SparcpointResource
{
    public string[] SubscriptionResourceIds { get; set; } = Array.Empty<string>();
    public string[] OrganizationResourceIds { get; set; } = Array.Empty<string>();
}
