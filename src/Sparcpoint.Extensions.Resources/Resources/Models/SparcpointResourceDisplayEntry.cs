namespace Sparcpoint.Extensions.Resources;

public class SparcpointResourceDisplayEntry
{
    public SparcpointResourceDisplayEntry(ScopePath resourceId, string resourceType, ResourcePermissions permissions, string displayName)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        Permissions = permissions;
        DisplayValue = displayName;
    }

    public ScopePath ResourceId { get; }
    public string ResourceType { get; }
    public ResourcePermissions Permissions { get; }
    public string DisplayValue { get; }
}