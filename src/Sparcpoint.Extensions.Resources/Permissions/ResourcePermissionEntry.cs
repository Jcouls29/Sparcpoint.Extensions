using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public record ResourcePermissionEntry
{
    public string AccountId { get; set; } = string.Empty;
    public PermissionEntry Permission { get; set; } = PermissionEntry.Empty;
}