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
}