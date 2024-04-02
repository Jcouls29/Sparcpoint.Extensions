namespace Sparcpoint.Extensions.Permissions;

public sealed class AccountPermissionsBuilder
{
    private readonly List<AccountPermissionEntry> _Entries;
    private readonly string _AccountId;
    private ScopePath _CurrentScope;

    public AccountPermissionsBuilder(string accountId, ScopePath? scope = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        _AccountId = accountId;
        _CurrentScope = scope ?? ScopePath.RootScope;
        _Entries = new List<AccountPermissionEntry>();
    }

    public AccountPermissionsBuilder Allow(string key, Dictionary<string, string>? metadata = null)
    {
        _Entries.Add(new AccountPermissionEntry(_AccountId, _CurrentScope, new PermissionEntry(key, PermissionValue.Allow, metadata)));
        return this;
    }

    public AccountPermissionsBuilder Deny(string key, Dictionary<string, string>? metadata = null)
    {
        _Entries.Add(new AccountPermissionEntry(_AccountId, _CurrentScope, new PermissionEntry(key, PermissionValue.Deny, metadata)));
        return this;
    }

    public AccountPermissionsBuilder Scope(ScopePath scope)
    {
        _CurrentScope = scope;
        return this;
    }

    public IEnumerable<AccountPermissionEntry> GetEntries()
    {
        var result = _Entries.ToArray();
        _Entries.Clear();
        return result;
    }
}