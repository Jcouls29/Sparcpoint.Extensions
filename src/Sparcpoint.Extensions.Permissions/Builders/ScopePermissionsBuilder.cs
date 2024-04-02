namespace Sparcpoint.Extensions.Permissions;

public sealed class ScopePermissionsBuilder
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly ScopePath _CurrentScope;

    private ScopePermissionsBuilder(ScopePath? scope)
    {
        _Entries = new List<AccountPermissionEntry>();
        _CurrentScope = scope ?? ScopePath.RootScope;
    }

    public ScopePermissionsBuilder Account(string accountId, Func<AccountPermissionsBuilder, AccountPermissionsBuilder> configure)
    {
        var builder = new AccountPermissionsBuilder(accountId, _CurrentScope);
        configure(builder);
        var results = builder.GetEntries();
        if (results.Any())
            _Entries.AddRange(results);
        
        return this;
    }

    public ScopePermissionsBuilder Scope(ScopePath scope, Func<ScopePermissionsBuilder, ScopePermissionsBuilder> configure)
    {
        var builder = new ScopePermissionsBuilder(scope);
        configure(builder);
        var results = builder.GetEntries();
        if (results.Any())
            _Entries.AddRange(results);

        return this;
    }

    public IEnumerable<AccountPermissionEntry> GetEntries()
    {
        var result = _Entries.ToArray();
        _Entries.Clear();
        return result;
    }

    public static ScopePermissionsBuilder Create(ScopePath scope)
    {
        return new ScopePermissionsBuilder(scope);
    }
}
