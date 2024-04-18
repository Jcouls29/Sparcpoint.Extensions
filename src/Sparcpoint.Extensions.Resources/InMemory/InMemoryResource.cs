namespace Sparcpoint.Extensions.Resources.InMemory;

internal class InMemoryResource
{
    public ScopePath ResourceId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public object? Resource { get; set; } = null;
    public ResourcePermissions Permissions { get; set; } = new();
}
