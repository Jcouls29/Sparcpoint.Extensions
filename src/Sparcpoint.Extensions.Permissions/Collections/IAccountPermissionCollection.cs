namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionCollection : IAsyncEnumerable<PermissionEntry>
{
    string AccountId { get; }
    ScopePath CurrentScope { get; }

    Task SetRangeAsync(IEnumerable<PermissionEntry> entries);
    Task RemoveAsync(IEnumerable<string> key);
    Task ClearAsync();

    Task<bool> ContainsAsync(string key);
    Task<PermissionEntry?> FindAsync(string key);
}
