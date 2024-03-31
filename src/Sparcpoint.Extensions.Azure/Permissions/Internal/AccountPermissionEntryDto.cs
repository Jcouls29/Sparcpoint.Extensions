using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class AccountPermissionEntryDto
{
    public string AccountId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public PermissionValue Value { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    internal PermissionEntry GetEntry(ScopePath scope)
        => new PermissionEntry(Key, Value, scope, Metadata);
}