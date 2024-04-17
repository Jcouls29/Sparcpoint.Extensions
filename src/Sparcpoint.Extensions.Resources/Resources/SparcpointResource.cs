using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Resources;

public abstract class SparcpointResource
{
    [ResourceId, JsonIgnore]
    public ScopePath ResourceId { get; set; }
    [ResourcePermissions, JsonIgnore]
    public ResourcePermissions Permissions { get; set; } = new();

    public string ResourceType => ResourceTypeAttribute.GetResourceType(this.GetType());
    public string Name => ResourceId.Segments.Last();

    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    public static implicit operator ScopePath(SparcpointResource resource)
        => resource.ResourceId;
}

public class SparcpointResource<T> : SparcpointResource where T : class, new()
{
    public T Data { get; set; } = new();
}