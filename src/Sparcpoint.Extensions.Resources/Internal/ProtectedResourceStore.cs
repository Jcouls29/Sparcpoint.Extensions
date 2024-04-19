using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Sparcpoint.Extensions.Permissions;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Sparcpoint.Extensions.Resources;

internal class DefaultPermissionsCache : IPermissionCache
{
    private readonly IResourceStore _Store;
    private readonly IAppCache _Cache;
    private readonly List<ScopePath> _CacheKeys;
    private readonly object _Lock;
    private readonly TimeSpan _SlidingCacheExpiration = TimeSpan.FromMinutes(20);

    public DefaultPermissionsCache(IResourceStore store, IAppCache cache)
    {
        Ensure.ArgumentNotNull(store);
        Ensure.ArgumentNotNull(cache);

        _Store = store;
        _Cache = cache;
        _CacheKeys = new();
        _Lock = new object();
    }

    public async Task<ResourcePermissions?> GetAsync(ScopePath resourceId)
    {
        if (!_CacheKeys.Contains(resourceId))
        {
            lock(_Lock)
            {
                _CacheKeys.Add(resourceId);
            }
        }

        return await _Cache.GetOrAddAsync(resourceId, async () => await GetItemAsync(resourceId), _SlidingCacheExpiration);
    }

    public Task ResetAsync(ScopePath resourceId)
    {
        _Cache.Remove(resourceId);

        if (_CacheKeys.Contains(resourceId))
        {
            lock(_Lock)
            {
                _CacheKeys.Remove(resourceId);
            }
        }

        var foundKeys = _CacheKeys.Where(k => k.StartsWith(resourceId)).ToArray();
        foreach (var key in foundKeys)
            _Cache.Remove(key);

        if (foundKeys.Any())
        {
            lock (_Lock)
            {
                foreach (var key in foundKeys)
                    _CacheKeys.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    private async Task<ResourcePermissions?> GetItemAsync(ScopePath resourceId)
    {
        List<ResourcePermissions> permissions = new List<ResourcePermissions>();

        foreach(var potentialResourceId in resourceId.GetHierarchyAscending(false))
        {
            var found = await _Store.GetPermissionsAsync(potentialResourceId);
            if (found != null)
                permissions.Add(found);
        }

        if (permissions.Count == 0)
            return null;

        return ResourcePermissions.Merge(permissions.ToArray());
    }
}

internal class ProtectedResourceStore : IResourceStore
{
    private readonly IResourceStore _InnerStore;
    private readonly IPermissionCache _PermissionCache;
    private readonly string _AccountId;

    public ProtectedResourceStore(IResourceStore innerStore, IPermissionCache permissionCache, string accountId)
    {
        Ensure.ArgumentNotNull(innerStore);
        Ensure.ArgumentNotNull(permissionCache);
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        _InnerStore = innerStore;
        _PermissionCache = permissionCache;
        _AccountId = accountId;
    }

    public async Task DeleteAsync(params ScopePath[] resourceIds)
    {
        foreach(var rid in resourceIds)
        {
            if (!await HasPermissionAsync(rid, CommonPermissions.CanWritePermissions))
                throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{rid}' permissions.");
        }

        await _InnerStore.DeleteAsync(resourceIds);
    }

    public async Task<bool> ExistsAsync(ScopePath resourceId, string? resourceType = null)
    {
        if (!await HasPermissionAsync(resourceId, CommonPermissions.CanReadData))
            return false;

        return await _InnerStore.ExistsAsync(resourceId, resourceType);
    }

    public async Task<T?> GetAsync<T>(ScopePath resourceId) where T : SparcpointResource
    {
        if (!await HasPermissionAsync(resourceId, CommonPermissions.CanReadData))
            throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{resourceId}'.");

        var found = await _InnerStore.GetAsync<T>(resourceId);
        if (found == null)
            return null;

        if (!await HasPermissionAsync(resourceId, CommonPermissions.CanReadPermissions))
            found.Permissions = new();

        return found;
    }

    public async IAsyncEnumerable<SparcpointResourceEntry> GetChildEntriesAsync(ScopePath parentResourceId, int maxDepth = int.MaxValue, string[]? includeTypes = null)
    {
        await foreach(var item in _InnerStore.GetChildEntriesAsync(parentResourceId, maxDepth, includeTypes))
        {
            if (await HasPermissionAsync(item.ResourceId, CommonPermissions.CanReadData))
            {
                if (!await HasPermissionAsync(item.ResourceId, CommonPermissions.CanReadPermissions))
                    yield return new SparcpointResourceEntry(item.ResourceId, item.ResourceType, null);

                yield return item;
            }
        }
    }

    public async Task<ResourcePermissions?> GetPermissionsAsync(ScopePath resourceId)
    {
        if (!await HasPermissionAsync(resourceId, CommonPermissions.CanReadPermissions))
            throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{resourceId}' permissions.");

        return await _InnerStore.GetPermissionsAsync(resourceId);
    }

    public async Task SetAsync<T>(T data) where T : SparcpointResource
    {
        Ensure.ArgumentNotNull(data);

        if (!await HasPermissionAsync(data.ResourceId, CommonPermissions.CanWriteData))
            throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{data.ResourceId}'.");

        if (data.Permissions != null && !await HasPermissionAsync(data.ResourceId, CommonPermissions.CanWritePermissions))
            throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{data.ResourceId}' permissions.");

        await _InnerStore.SetAsync(data);

        if (data.Permissions != null)
            await _PermissionCache.ResetAsync(data.ResourceId);
    }

    public async Task SetPermissionsAsync(ScopePath resourceId, ResourcePermissions permissions)
    {
        if (!await HasPermissionAsync(resourceId, CommonPermissions.CanWritePermissions))
            throw new InvalidOperationException($"Account '{_AccountId}' does not have access to resource '{resourceId}' permissions.");

        await _InnerStore.SetPermissionsAsync(resourceId, permissions);
        await _PermissionCache.ResetAsync(resourceId);
    }

    private async Task<bool> HasPermissionAsync(ScopePath resourceId, string key)
    {
        var found = await _PermissionCache.GetAsync(resourceId);

        // This implies that the resource does NOT exist. In this case it is up for grabs
        if (found == null)
            return true;

        // After this point, we did find resource access and so we must apply permissions
        var entry = found.FirstOrDefault(e => e.AccountId == _AccountId && e.Permission.Key == key);
        return entry?.Permission.IsAllowed ?? false;
    }
}
