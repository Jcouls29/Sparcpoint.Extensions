namespace Sparcpoint.Extensions.Permissions;

public interface IScopePermissionView : IAsyncEnumerable<AccountPermissionEntry>
{
    ScopePath CurrentScope { get; }
}