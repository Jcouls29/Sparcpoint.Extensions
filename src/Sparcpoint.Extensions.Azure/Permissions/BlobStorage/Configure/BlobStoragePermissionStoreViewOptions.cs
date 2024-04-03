namespace Sparcpoint.Extensions.Azure.Permissions;

public sealed record BlobStoragePermissionStoreViewOptions
{
    public bool IncludeRootScopeInCalculations { get; set; } = false;

    public static BlobStoragePermissionStoreViewOptions Default { get; } = new();
}