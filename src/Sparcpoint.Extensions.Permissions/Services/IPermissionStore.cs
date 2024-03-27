namespace Sparcpoint.Extensions.Permissions;

public interface IPermissionStore
{
    IAccountPermissionCollection Get(string accountId, ScopePath? scope = null);
    IScopePermissionCollection Get(ScopePath scope);

    IAccountPermissionView GetView(string accountId, ScopePath? scope = null);
    IScopePermissionView GetView(ScopePath scope);
}