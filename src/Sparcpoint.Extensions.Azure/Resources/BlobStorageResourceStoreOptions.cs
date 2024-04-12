namespace Sparcpoint.Extensions.Azure.Resources;

public sealed class BlobStorageResourceStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}