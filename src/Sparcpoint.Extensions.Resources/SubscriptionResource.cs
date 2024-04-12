namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Subscription")]
public sealed class SubscriptionResource : SparcpointResource
{
    public string OwnerAccountId { get; set; }
    public string DisplayName { get; set; }
}
