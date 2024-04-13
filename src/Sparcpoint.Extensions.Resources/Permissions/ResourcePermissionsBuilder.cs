using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public class ResourcePermissionsBuilder
{
    private readonly string _AccountId;
    private readonly ResourcePermissions _Permissions;

    public ResourcePermissionsBuilder(string accountId)
    {
        Ensure.NotNullOrWhiteSpace(accountId);

        _AccountId = accountId;
        _Permissions = new();
    }

    public ResourcePermissionsBuilder With(string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Ensure.NotNullOrWhiteSpace(key);
        _Permissions.Add(new ResourcePermissionEntry
        {
            AccountId = _AccountId,
            Permission = new PermissionEntry(key, value, metadata)
        });

        return this;
    }

    public ResourcePermissionsBuilder Allow(string key, Dictionary<string, string>? metadata = null)
        => With(key, PermissionValue.Allow, metadata);

    public ResourcePermissionsBuilder Deny(string key, Dictionary<string, string>? metadata = null)
        => With(key, PermissionValue.Deny, metadata);

    public ResourcePermissions Build()
        => _Permissions;

    public static implicit operator ResourcePermissions(ResourcePermissionsBuilder builder)
        => builder.Build();
}