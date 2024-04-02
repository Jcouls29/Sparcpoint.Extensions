using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Objects;

namespace Sparcpoint.Extensions.Azure.Objects.BlobStorage;

internal static class Constants
{
    public const string TYPE_KEY = "DotNetType_FullName";
    public const string NAME_KEY = "Name";
}

internal class BlobStorageObjectStore<T> : IObjectStore<T> where T : class, ISparcpointObject
{
    private readonly BlobContainerClient _Client;
    private readonly BlobStorageObjectStoreOptions _Options;

    public BlobStorageObjectStore(BlobContainerClient client, BlobStorageObjectStoreOptions options)
    {
        _Client = client;
        _Options = options;
    }

    public async Task DeleteAsync(IEnumerable<ScopePath> ids)
    {
        await _Client.CreateIfNotExistsAsync();

        var values = ids.Select(x => x.Append(_Options.Filename).ToString());
        await _Client.BulkDeleteAsync(values);
    }

    public async Task<T?> FindAsync(ScopePath id)
    {
        await _Client.CreateIfNotExistsAsync();

        var blobName = id.Append(_Options.Filename);
        var bc = _Client.GetBlobClient(blobName);

        return await bc.GetAsJsonAsync<T>();
    }

    public async Task UpsertAsync(IEnumerable<T> o)
    {
        await _Client.CreateIfNotExistsAsync();

        // TODO: Atomicity?
        foreach (var x in o)
        {
            await UpsertAsync(x);
        }
    }

    private async Task UpsertAsync(T o)
    {
        var blobName = o.Id.Append(_Options.Filename).ToString();
        var bc = _Client.GetBlobClient(blobName);

        var typeName = SparcpointObjectAttribute.GetTypeName(typeof(T));
        
        await bc.UpdateAsJsonAsync(o, tags: new Dictionary<string, string>
        {
            [Constants.TYPE_KEY] = typeName.EncodeBlobTagValue(),
            [Constants.NAME_KEY] = o.Name.EncodeBlobTagValue(),
        });
    }
}
