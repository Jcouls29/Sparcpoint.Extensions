namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionCollection : IAsyncEnumerable<PermissionEntry>
{
    string AccountId { get; }
    ScopePath CurrentScope { get; }

    Task SetAsync(string key, PermissionValue value, Dictionary<string, string>? metadata = null);
    Task RemoveAsync(string key);
    Task<bool> ContainsAsync(string key);
    Task ClearAsync();
    Task<PermissionEntry> GetAsync(string key);
}
