namespace Sparcpoint.Extensions.Resources;

public interface IPermissionCache
{
    Task<ResourcePermissions?> GetAsync(ScopePath resourceId);
    Task ResetAsync(ScopePath resourceId);
}
