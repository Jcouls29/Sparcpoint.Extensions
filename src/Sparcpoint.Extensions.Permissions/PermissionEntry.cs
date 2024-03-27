namespace Sparcpoint.Extensions.Permissions;

public record PermissionEntry
{
    public PermissionEntry(string key, PermissionValue value, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
    {
        Assertions.NotEmptyOrWhitespace(key);

        Scope = scope ?? ScopePath.RootScope;
        Key = key;
        Value = value;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    public ScopePath Scope { get; }
    public string Key { get; }
    public PermissionValue Value { get; }
    public Dictionary<string, string> Metadata { get; }

    public bool? IsAllowed => (Value == PermissionValue.None) ? null : ((Value == PermissionValue.Allow) ? true : false);

    public static PermissionEntry Create(string key, PermissionValue value, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => new PermissionEntry(key, value, scope, metadata);
    public static PermissionEntry Allow(string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => Create(key, PermissionValue.Allow, scope, metadata);
    public static PermissionEntry Deny(string key, ScopePath? scope = null, Dictionary<string, string>? metadata = null)
        => Create(key, PermissionValue.Deny, scope, metadata);
}
