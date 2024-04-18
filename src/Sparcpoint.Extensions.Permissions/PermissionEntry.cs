namespace Sparcpoint.Extensions.Permissions;

public class Permissions : List<PermissionEntry> { }

public record PermissionEntry
{
    public PermissionEntry(string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        Key = key;
        Value = value;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    public string Key { get; }
    public PermissionValue Value { get; }
    public Dictionary<string, string> Metadata { get; }

    public bool? IsAllowed => (Value == PermissionValue.None) ? null : ((Value == PermissionValue.Allow) ? true : false);

    public static PermissionEntry Create(string key, PermissionValue value, Dictionary<string, string>? metadata = null)
        => new PermissionEntry(key, value, metadata);
    public static PermissionEntry Allow(string key, Dictionary<string, string>? metadata = null)
        => Create(key, PermissionValue.Allow, metadata);
    public static PermissionEntry Deny(string key, Dictionary<string, string>? metadata = null)
        => Create(key, PermissionValue.Deny, metadata);

    public static PermissionEntry Empty = new PermissionEntry("None", PermissionValue.None, null);
}
