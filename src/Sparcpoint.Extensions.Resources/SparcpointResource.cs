using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Resources;

public class SparcpointResource<T>
{
    public SparcpointResource(T data, AccountPermissions permissions)
    {
        Data = data;
        Permissions = permissions;
    }

    public T Data { get; }
    public AccountPermissions Permissions { get; }
}