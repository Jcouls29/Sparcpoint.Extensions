using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Objects;

namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

internal class BlobStorageObjectStoreFactory : IObjectStoreFactory
{
    private readonly BlobContainerClient _Client;
    private readonly BlobStorageObjectStoreOptions _Options;

    public BlobStorageObjectStoreFactory(BlobContainerClient client, BlobStorageObjectStoreOptions options)
    {
        _Client = client;
        _Options = options;
    }

    public IObjectQuery<T> CreateQuery<T>() where T : class, ISparcpointObject
        => new BlobStorageObjectQuery<T>(_Client, _Options);

    public IObjectStore<T> CreateStore<T>() where T : class, ISparcpointObject
        => new BlobStorageObjectStore<T>(_Client, _Options);
}
