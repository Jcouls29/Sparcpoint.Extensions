using Sparcpoint.Extensions.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Sparcpoint.Extensions.Resources;

public interface IResourceStore
{
    Task SetAsync<T>(T data) where T : SparcpointResource;
    Task<T?> GetAsync<T>(ScopePath resourceId) where T : SparcpointResource;
    Task DeleteAsync(params ScopePath[] resourceIds);
    Task<bool> ExistsAsync(ScopePath resourceId, string? resourceType = null);

    Task<ResourcePermissions?> GetPermissionsAsync(ScopePath resourceId);
    Task SetPermissionsAsync(ScopePath resourceId, ResourcePermissions permissions);

    IAsyncEnumerable<SparcpointResourceEntry> GetChildEntriesAsync(ScopePath parentResourceId, int maxDepth = int.MaxValue, string[]? includeTypes = null);
}
