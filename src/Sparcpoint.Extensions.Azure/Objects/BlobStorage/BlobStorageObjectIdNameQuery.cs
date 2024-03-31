using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Objects;

namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

internal class BlobStorageObjectIdNameQuery : IObjectIdNameQuery
{
    private readonly BlobStorageObjectQuery _Query;
    public BlobStorageObjectIdNameQuery(BlobContainerClient client, BlobStorageObjectStoreOptions options)
    {
        _Query = new BlobStorageObjectQuery(client, options);
    }

    public async IAsyncEnumerable<SparcpointObjectId> RunAsync(ObjectQueryParameters parameters)
    {
        // TODO: Performance Inefficient. Need to re-write to load the object only when necessary.
        //       Should be able to do checking on all the objects with tags only (unless property
        //       checks are specified
        await foreach (var f in _Query.RunAsync(parameters))
        {
            yield return new SparcpointObjectId(f.Id, f.Name);
        }
    }
}
