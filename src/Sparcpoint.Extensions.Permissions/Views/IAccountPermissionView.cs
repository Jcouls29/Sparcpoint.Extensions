namespace Sparcpoint.Extensions.Permissions;

public interface IAccountPermissionView : IAsyncEnumerable<PermissionEntry>
{
    string AccountId { get; }
    ScopePath CurrentScope { get; }
}