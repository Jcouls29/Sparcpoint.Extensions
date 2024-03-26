namespace Sparcpoint.Extensions.Permissions;

public interface IScopePermissionCollection : IAsyncEnumerable<AccountPermissionEntry>
{
    ScopePath CurrentScope { get; }

    Task SetAsync(string accountId, string key, PermissionValue value, Dictionary<string, string>? metadata = null);
    Task RemoveAsync(string accountId, string key);
    Task<bool> ContainsAsync(string accountId, string key);
    Task ClearAsync();
    Task<IAsyncEnumerable<AccountPermissionEntry>> GetAsync(string key);
}
