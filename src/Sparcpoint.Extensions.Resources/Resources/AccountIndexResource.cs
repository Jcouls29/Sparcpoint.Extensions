namespace Sparcpoint.Extensions.Resources;

[ResourceType("Account.Index")]
public sealed class AccountIndexResource : SparcpointResource
{
    public const string RESOURCE_FORMAT = "/accounts/{0}";

    public List<AccountIndexEntry> Resources { get; set; } = new();
}
