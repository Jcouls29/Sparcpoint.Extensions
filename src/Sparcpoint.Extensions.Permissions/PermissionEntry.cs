namespace Sparcpoint.Extensions.Permissions;

public readonly record struct PermissionEntry
{
    public PermissionEntry(string key, PermissionValue value, ScopePath? scope = null)
    {
        Assertions.NotEmptyOrWhitespace(key);

        Scope = scope ?? ScopePath.RootScope;
        Key = key;
        Value = value;
    }

    public ScopePath Scope { get; }
    public string Key { get; }
    public PermissionValue Value { get; }

    public bool? IsAllowed => (Value == PermissionValue.None) ? null : ((Value == PermissionValue.Allow) ? true : false);

    public static PermissionEntry Empty { get; } = new PermissionEntry();
}
