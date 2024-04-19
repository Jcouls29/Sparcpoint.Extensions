using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public record SparcpointResourceEntry
{
    public SparcpointResourceEntry(ScopePath resourceId, string resourceType, ResourcePermissions? permissions)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        Permissions = permissions;
    }

    public ScopePath ResourceId { get; }
    public string ResourceType { get; }
    public ResourcePermissions? Permissions { get; }
}
