using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageAccountPermissionCollection : IAccountPermissionCollection
{
    private readonly BlobContainerClient _ContainerClient;
    private readonly BlobClient _Client;

    public BlobStorageAccountPermissionCollection(BlobContainerClient client, string accountId, ScopePath? scope, string filename)
    {
        AccountId = accountId;
        CurrentScope = scope ?? ScopePath.RootScope;

        _ContainerClient = client;
        _Client = client.GetBlobClient(CurrentScope.Append(filename));
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public async Task ClearAsync()
    {
        await _ContainerClient.EnsureCreatedAsync();

        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return null;

            var found = coll.Where(e => e.AccountId == AccountId).ToArray();
            foreach (var f in found)
                coll.Remove(f);

            if (!coll.Any())
                return null;

            return coll;
        });
    }

    public async Task<bool> ContainsAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        await _ContainerClient.EnsureCreatedAsync();

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            return false;
        
        return coll.Any(e => e.AccountId == AccountId && e.Key == key);
    }

    public async Task<PermissionEntry?> FindAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        await _ContainerClient.EnsureCreatedAsync();

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            throw new KeyNotFoundException($"Entry {key} not found.");

        var found = coll.FirstOrDefault(e => e.AccountId == AccountId && e.Key == key);
        if (found == null)
            return null;

        return new PermissionEntry(found.Key, found.Value, found.Metadata);
    }

    public async IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await _ContainerClient.EnsureCreatedAsync();

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            yield break;

        var accountItems = coll.Where(e => e.AccountId == AccountId).ToArray();
        foreach (var item in accountItems)
        {
            yield return new PermissionEntry(item.Key, item.Value, item.Metadata);
        }
    }

    public async Task RemoveAsync(IEnumerable<string> keys)
    {
        foreach(var k in keys)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(k);
        }

        await _ContainerClient.EnsureCreatedAsync();
        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return null;

            foreach(var k in keys)
            {
                var found = coll.Where(e => e.AccountId == AccountId && e.Key == k).ToArray();
                foreach (var f in found)
                {
                    coll.Remove(f);
                }
            }

            return coll;
        });
    }

    public async Task SetRangeAsync(IEnumerable<PermissionEntry> entries)
    {
        foreach(var e in entries)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(e.Key);
        }

        await _ContainerClient.EnsureCreatedAsync();
        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                coll = new List<AccountPermissionEntryDto>();

            foreach (var e in entries)
            {
                var found = coll.FirstOrDefault(p => p.AccountId == AccountId && p.Key == e.Key);
                if (found == null)
                {
                    found = new();
                    coll.Add(found);
                }

                found.AccountId = AccountId;
                found.Key = e.Key;
                found.Value = e.Value;
                found.Metadata = e.Metadata ?? new();
            }

            return coll;
        });
    }
}
