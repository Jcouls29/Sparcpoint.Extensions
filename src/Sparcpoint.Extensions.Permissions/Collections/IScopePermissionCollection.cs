namespace Sparcpoint.Extensions.Permissions;

public interface IScopePermissionCollection : IAsyncEnumerable<AccountPermissionEntry>
{
    Task SetRangeAsync(IEnumerable<AccountPermissionEntry> entries);
    Task RemoveAsync(IEnumerable<AccountPermissionEntry> entries);
    Task ClearAsync();

    Task<bool> ContainsAsync(AccountPermissionEntry entry);

    IAsyncEnumerable<AccountPermissionEntry> GetAsync(ScopePath scope);
    IAccountPermissionCollection Get(string accountId, ScopePath? scope = null);
}
