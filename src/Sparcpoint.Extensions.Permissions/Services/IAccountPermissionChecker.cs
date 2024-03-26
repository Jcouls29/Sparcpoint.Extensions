namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionChecker
{
    Task<bool?> IsAllowedAsync(string accountId, string key, ScopePath? scope = null);
}
