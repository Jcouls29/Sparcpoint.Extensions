using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Services.InMemory;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageScopePermissionCollection : IScopePermissionCollection
{
    private readonly BlobContainerClient _ContainerClient;
    private readonly string _Filename;

    public BlobStorageScopePermissionCollection(BlobContainerClient containerClient, string filename)
    {
        _ContainerClient = containerClient;
        _Filename = filename;
    }

    public async Task ClearAsync()
    {
        await _ContainerClient.CreateIfNotExistsAsync();

        await foreach(var client in GetAllPermissionBlobs())
        {
            await client.DeleteIfExistsAsync();
        }
    }

    public async Task<bool> ContainsAsync(AccountPermissionEntry entry)
    {
        EnsureEntryValid(entry);

        await _ContainerClient.CreateIfNotExistsAsync();
        var client = GetScopeClient(entry.Scope);
        if (!await client.ExistsAsync())
            return false;

        var coll = await client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            return false;

        return coll.Any(e => e.AccountId == entry.AccountId && e.Key == entry.Entry.Key);
    }

    public async IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await _ContainerClient.CreateIfNotExistsAsync();

        await foreach(var client in GetAllPermissionBlobs())
        {
            var coll = await client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
            if (coll == null)
                continue;

            foreach (var item in coll)
                yield return new AccountPermissionEntry(item.AccountId, ScopePath.Parse(client.Name).Back(), new PermissionEntry(item.Key, item.Value, item.Metadata));
        }
    }

    public async Task SetRangeAsync(IEnumerable<AccountPermissionEntry> entries)
    {
        EnsureEntriesValid(entries);

        await _ContainerClient.CreateIfNotExistsAsync();

        var groups = entries.GroupBy(e => e.Scope);
        foreach(var scope in groups)
        {
            var client = GetScopeClient(scope.Key);
            await client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
            {
                if (coll == null)
                    coll = new List<AccountPermissionEntryDto>();

                foreach(var item in scope)
                {
                    var found = coll.FirstOrDefault(e => e.AccountId == item.AccountId && e.Key == item.Entry.Key);
                    if (found == null)
                    {
                        found = new();
                        coll.Add(found);
                    }

                    found.AccountId = item.AccountId;
                    found.Key = item.Entry.Key;
                    found.Value = item.Entry.Value;
                    found.Metadata = item.Entry.Metadata ?? new();
                }

                return coll;
            });
        }
    }

    public async Task RemoveAsync(IEnumerable<AccountPermissionEntry> entries)
    {
        EnsureEntriesValid(entries);

        await _ContainerClient.CreateIfNotExistsAsync();

        var groups = entries.GroupBy(e => e.Scope);

        foreach(var scope in groups)
        {
            var client = GetScopeClient(scope.Key);
            await client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
            {
                if (coll == null)
                    return null;

                foreach(var item in scope)
                {
                    var found = coll.Where(e => e.AccountId == item.AccountId && e.Key == item.Entry.Key).ToArray();
                    foreach (var f in found)
                        coll.Remove(f);
                }

                return coll;
            });
        }
    }

    public async IAsyncEnumerable<AccountPermissionEntry> GetAsync(ScopePath scope)
    {
        await _ContainerClient.CreateIfNotExistsAsync();

        var client = GetScopeClient(scope);
        var coll = await client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            yield break;

        foreach(var item in coll)
        {
            yield return new AccountPermissionEntry(item.AccountId, scope, new PermissionEntry(item.Key, item.Value, item.Metadata));
        }
    }

    public IAccountPermissionCollection Get(string accountId, ScopePath? scope = null)
    {
        return new BlobStorageAccountPermissionCollection(_ContainerClient, accountId, scope, _Filename);
    }

    private async IAsyncEnumerable<BlobClient> GetAllPermissionBlobs()
    {
        await _ContainerClient.CreateIfNotExistsAsync();

        await foreach(var b in _ContainerClient.GetBlobsAsync())
        {
            if (b.Name.EndsWith("/" + _Filename))
                yield return _ContainerClient.GetBlobClient(b.Name);
        }
    }

    private BlobClient GetScopeClient(ScopePath scope)
        => _ContainerClient.GetBlobClient(scope.Append(_Filename));

    private void EnsureEntriesValid(IEnumerable<AccountPermissionEntry> entries)
    {
        foreach (var e in entries)
        {
            EnsureEntryValid(e);
        }
    }

    private void EnsureEntryValid(AccountPermissionEntry entry)
    {
        Ensure.ArgumentNotNull(entry);
        Ensure.ArgumentNotNullOrWhiteSpace(entry.AccountId);
        Ensure.ArgumentNotNull(entry.Entry);
        Ensure.ArgumentNotNullOrWhiteSpace(entry.Entry.Key);
    }
}
