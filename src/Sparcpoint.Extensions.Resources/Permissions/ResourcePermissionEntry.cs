using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public class ResourcePermissionEntry
{
    public string AccountId { get; set; }
    public PermissionEntry Permission { get; set; }
}