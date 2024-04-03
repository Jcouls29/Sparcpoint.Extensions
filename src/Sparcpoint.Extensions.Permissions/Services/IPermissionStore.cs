namespace Sparcpoint.Extensions.Permissions;

public interface IPermissionStore
{
    IScopePermissionCollection Permissions { get; }
    IScopePermissionView GetView(ScopePath scope, bool includeRootScope = false, string[]? keys = null, string[]? accountIds = null);
}
