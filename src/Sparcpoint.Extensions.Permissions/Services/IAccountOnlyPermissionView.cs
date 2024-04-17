namespace Sparcpoint.Extensions.Permissions;

public interface IAccountOnlyPermissionView : IScopePermissionView
{
    string AccountId { get; }
}