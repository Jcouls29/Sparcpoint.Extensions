namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

public sealed class BlobStorageObjectStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;

    // TODO: Allow for no filename and instead, use the scope directly as the filename.
    //       Doing this severely limits the use for the container and can then ONLY
    //       be used for objects or else risk of data mismatches being returned
    // NOTE: To alleviate this issue, tags / metadata COULD be used to differentiate
    //       between object files and other files. Investigation needed to see
    //       performance impacts
    public string Filename { get; set; } = ".object";
}
