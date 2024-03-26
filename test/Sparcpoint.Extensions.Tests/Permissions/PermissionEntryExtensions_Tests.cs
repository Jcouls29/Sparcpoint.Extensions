using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Tests.Permissions;

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
    public void When_permission_missing_returns_none(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] { AccountPermissionEntry.Create("ACCOUNT", "OTHER_KEY", PermissionValue.Allow, scope: scope) });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/OtherScope")]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    public void When_exact_permission_exists_it_returns(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: scope),
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Allow, value);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/OtherScope")]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    public void When_exact_permission_exists_it_returns_deny(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: scope),
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Deny, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void When_inherited_permission_exists_it_returns(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: "/OtherScope"),
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Allow, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void When_inherited_permission_exists_it_returns_deny(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: "/OtherScope"),
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Deny, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void Exact_permission_overrides_inherited_permission(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: scope)
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Allow, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope")]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void Exact_permission_overrides_inherited_permission_deny(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: scope)
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Deny, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void More_specific_permission_overrides_inherited_permission(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: "/OtherScope/ChildScope")
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Allow, value);
    }

    [Theory]
    [InlineData("/OtherScope/ChildScope/2")]
    [InlineData("/OtherScope/ChildScope/2/3/4/5")]
    public void More_specific_permission_overrides_inherited_permission_deny(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Deny, scope: "/OtherScope/ChildScope")
        });
        var value = entries.CalculatePermissionValue(scope, "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.Deny, value);
    }

    [Fact]
    public void No_account_overlap()
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT_OTHER", "KEY", PermissionValue.Allow, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT_OTHER", "KEY", PermissionValue.Deny, scope: "/OtherScope/ChildScope")
        });
        var value = entries.CalculatePermissionValue("/OtherScope/ChildScope/2", "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }

    [Fact]
    public void No_key_overlap()
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY_OTHER", PermissionValue.Allow, scope: "/OtherScope"),
            AccountPermissionEntry.Create("ACCOUNT", "KEY_OTHER", PermissionValue.Deny, scope: "/OtherScope/ChildScope")
        });
        var value = entries.CalculatePermissionValue("/OtherScope/ChildScope/2", "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }

    [Theory]
    [InlineData("/scope")]
    [InlineData("/scope/OtherScope")]
    [InlineData("/scope/OtherScope/ChildScope/2")]
    public void No_scope_overlap(string scope)
    {
        var entries = new List<AccountPermissionEntry>(new[] {
            AccountPermissionEntry.Create("ACCOUNT", "KEY", PermissionValue.Allow, scope: scope),
        });
        var value = entries.CalculatePermissionValue("/OtherScope/ChildScope/2", "ACCOUNT", "KEY");

        Assert.Equal(PermissionValue.None, value);
    }
}