namespace Sparcpoint.Extensions.Permissions;

public record AccountPermissionEntry
{
    public AccountPermissionEntry(string accountId, PermissionEntry entry)
    {
        Assertions.NotEmptyOrWhitespace(accountId);

        AccountId = accountId;
        Entry = entry;
    }

    public string AccountId { get; }
    public PermissionEntry Entry { get; }

    public static AccountPermissionEntry Create(string accountId, string key, PermissionValue value, ScopePath? scope = null, Dictionary<string, string> metadata = null)
        => new AccountPermissionEntry(accountId, PermissionEntry.Create(key, value, scope, metadata));
}