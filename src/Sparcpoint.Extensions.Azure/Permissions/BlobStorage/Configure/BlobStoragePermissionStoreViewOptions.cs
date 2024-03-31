namespace Sparcpoint.Extensions.Azure.Permissions;

public sealed class BlobStoragePermissionStoreViewOptions
{
    public bool IncludeRootScopeInCalculations { get; set; } = false;

    public static BlobStoragePermissionStoreViewOptions Default { get; } = new();
}