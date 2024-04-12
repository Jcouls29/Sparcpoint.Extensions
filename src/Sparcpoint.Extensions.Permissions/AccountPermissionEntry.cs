namespace Sparcpoint.Extensions.Permissions;

public record AccountPermissionEntry
{
    public const string ALL_ACCOUNTS_ID = "*";

    public AccountPermissionEntry(string accountId, ScopePath scope, PermissionEntry entry)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        AccountId = accountId;
        Scope = scope;
        Entry = entry;
    }

    public string AccountId { get; }
    public ScopePath Scope { get; }
    public PermissionEntry Entry { get; }

    public static AccountPermissionEntry Create(string accountId, string key, PermissionValue value, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => new AccountPermissionEntry(accountId, scope ?? ScopePath.RootScope, PermissionEntry.Create(key, value, metadata));

    public static AccountPermissionEntry CreateWithAllAccounts(string key, PermissionValue value, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => new AccountPermissionEntry(ALL_ACCOUNTS_ID, scope ?? ScopePath.RootScope, PermissionEntry.Create(key, value, metadata));

    public static AccountPermissionEntry Allow(string accountId, string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => Create(accountId, key, PermissionValue.Allow, scope, metadata);

    public static AccountPermissionEntry Deny(string accountId, string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => Create(accountId, key, PermissionValue.Deny, scope, metadata);

    public static AccountPermissionEntry AllowAll(string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => CreateWithAllAccounts(key, PermissionValue.Allow, scope, metadata);

    public static AccountPermissionEntry DenyAll(string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => CreateWithAllAccounts(key, PermissionValue.Deny, scope, metadata);
}