namespace Sparcpoint.Extensions.Permissions;

public interface IPermissionStore
{
    IScopePermissionCollection Permissions { get; }

    IAccountPermissionView GetView(string accountId, ScopePath? scope = null);
    IScopePermissionView GetView(ScopePath scope, bool includeRootScope = false);
}
