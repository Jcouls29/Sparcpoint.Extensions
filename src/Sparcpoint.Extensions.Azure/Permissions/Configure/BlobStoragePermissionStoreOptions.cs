namespace Sparcpoint.Extensions.Azure.Permissions;

public sealed class BlobStoragePermissionStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;

    public string Filename { get; set; } = ".permissions";

    public BlobStoragePermissionStoreViewOptions View { get; set; } = new();
}
