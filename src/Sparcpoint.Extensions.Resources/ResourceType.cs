using System.Diagnostics.CodeAnalysis;

namespace Sparcpoint.Extensions.Resources;

public readonly record struct ResourceType(string SubscriptionId, string OrganizationId, string ProviderName, string ResourceTypeName)
{
    private static readonly string[] INVALID_NAMES = new[] { SUBSCRIPTIONS, ORGANIZATIONS, PROVIDERS };
    private const string SUBSCRIPTIONS = "subscriptions";
    private const string ORGANIZATIONS = "organizations";
    private const string PROVIDERS = "providers";

    public ScopePath GetScope()
        => ScopePath.Parse("/subscriptions/" + SubscriptionId + "/organizations/" + OrganizationId + "/providers/" + ProviderName + "/" + ResourceTypeName);

    internal static bool TryParse(string[] segments, [NotNullWhen(true)] out ResourceType? resourceType)
    {
        resourceType = null;
        if (segments.Length < 7)
            return false;

        if (segments[0] != SUBSCRIPTIONS)
            return false;
        var subscriptionName = segments[1];
        Ensure.NotValues(INVALID_NAMES, subscriptionName);

        if (segments[2] != ORGANIZATIONS)
            return false;
        var organizationName = segments[3];
        Ensure.NotValues(INVALID_NAMES, organizationName);

        if (segments[4] != PROVIDERS)
            return false;
        var providerName = segments[5];
        Ensure.NotValues(INVALID_NAMES, providerName);

        var name = segments[6];
        Ensure.NotValues(INVALID_NAMES, name);

        resourceType = new ResourceType(subscriptionName, organizationName, providerName, name);
        return true;
    }
}
