using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public class SparcpointResourceEntry
{
    public SparcpointResourceEntry(ScopePath resourceId, string resourceType, AccountPermissions permissions)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        Permissions = permissions;
    }

    public ScopePath ResourceId { get; }
    public string ResourceType { get; }
    public AccountPermissions Permissions { get; }
}