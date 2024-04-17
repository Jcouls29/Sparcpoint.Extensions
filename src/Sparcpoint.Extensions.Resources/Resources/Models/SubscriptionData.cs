namespace Sparcpoint.Extensions.Resources;

[ResourceType("Sparcpoint.Subscription")]
public sealed record SubscriptionData
{
    public string OwnerAccountId { get; set; }
    public string DisplayName { get; set; }

    private const string RESOURCE_FORMAT = "/subscriptions/{0}";
    internal static ScopePath CreateResourceId(string name)
        => string.Format(RESOURCE_FORMAT, name);
}