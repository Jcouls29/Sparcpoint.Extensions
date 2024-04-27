namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Subscription")]
public sealed record SubscriptionData
{
    [ResourceId]
    public ScopePath ResourceId { get; set; }
    public string OwnerAccountId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    private const string RESOURCE_FORMAT = "/subscriptions/{0}";
    internal static ScopePath CreateResourceId(string name)
        => string.Format(RESOURCE_FORMAT, name);
}