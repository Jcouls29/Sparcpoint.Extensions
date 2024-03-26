namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionStore
{
    Task SetAsync(string accountId, PermissionEntry entry);
    Task<IEnumerable<PermissionEntry>> GetEntriesAsync(string accountId, ScopePath? scope = null);
    Task<IEnumerable<ScopePath>> GetScopesAsync(string accountId);
    Task<PermissionEntry[]> GetPermissionChainAsync(string accountId, ScopePath scope, string key);
}
