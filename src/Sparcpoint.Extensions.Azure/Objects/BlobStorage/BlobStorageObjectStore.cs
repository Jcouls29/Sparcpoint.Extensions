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

    public async Task DeleteAsync(IEnumerable<T> o)
    {
        var ids = o.Select(x => x.Id.Append(_Options.Filename).ToString());
        await _Client.BulkDeleteAsync(ids);
    }

    public async Task<T?> FindAsync(ScopePath id)
    {
        var blobName = id.Append(_Options.Filename);
        var bc = _Client.GetBlobClient(blobName);

        return await bc.GetAsJsonAsync<T>();
    }

    public async Task UpsertAsync(IEnumerable<T> o)
    {
        // TODO: Atomicity?
        foreach(var x in o)
        {
            await UpsertAsync(x);
        }
    }

    private async Task UpsertAsync(T o)
    {
        var blobName = o.Id.Append(_Options.Filename).ToString();
        var bc = _Client.GetBlobClient(blobName);

        await bc.UpdateAsJsonAsync(o, tags: new Dictionary<string, string>
        {
            [Constants.TYPE_KEY] = typeof(T).FullName ?? throw new InvalidOperationException($"ISparcointObject type is invalid."),
            [Constants.NAME_KEY] = o.Name,
        });
    }
}
