using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Tests.Permissions;

public class InMemoryPermissionStore_Tests
{

}

public class PermissionEntryExtensions_Tests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/OtherScope/ChildScope")]
    public void When_no_permissions_returns_none_RootScope(string? scope)
    {
        List<AccountPermissionEntry> entries = new List<AccountPermissionEntry>();
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/OtherScope/ChildScope")]
    public void When_permission_missing_returns_none(string? scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] { AccountPermissionEntry.Create("ACCOUNT", "OTHER_KEY", PermissionValue.Allow, scope: scope) });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }
}