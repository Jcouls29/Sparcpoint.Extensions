using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Services.InMemory;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageScopePermissionCollection : IScopePermissionCollection
{
    private readonly BlobClient _Client;
    public BlobStorageScopePermissionCollection(BlobContainerClient containerClient, ScopePath scope, string filename)
    {
        _Client = containerClient.GetBlobClient(scope.Append(filename));
        CurrentScope = scope;
    }

    public ScopePath CurrentScope { get; }

    public async Task ClearAsync()
    {
        await _Client.DeleteIfExistsAsync();
    }

    public async Task<bool> ContainsAsync(string accountId, string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            return false;

        return coll.Any(e => e.AccountId == accountId && e.Key == key);
    }

    public async Task<IAsyncEnumerable<AccountPermissionEntry>> GetAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            coll = new();

        var found = coll
            .Where(e => e.Key == key)
            .Select(e => new AccountPermissionEntry(e.AccountId, e.GetEntry(CurrentScope)))
            .ToList();

        return new SynchronousAsyncEnumerable<AccountPermissionEntry>(found);
    }

    public async IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            coll = new();

        foreach(var f in coll)
        {
            yield return new AccountPermissionEntry(f.AccountId, f.GetEntry(CurrentScope));
        }
    }

    public async Task RemoveAsync(string accountId, string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);
        
        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return null;

            var found = coll.Where(e => e.AccountId == accountId && e.Key == key).ToArray();
            foreach(var f in found)
            {
                coll.Remove(f);
            }

            return coll;
        });
    }

    public async Task SetAsync(string accountId, string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return new List<AccountPermissionEntryDto>();

            var found = coll.FirstOrDefault(e => e.AccountId == accountId && e.Key == key);
            if (found == null)
            {
                found = new();
                coll.Add(found);
            }

            found.AccountId = accountId;
            found.Key = key;
            found.Value = value;
            found.Metadata = metadata ?? new();

            return coll;
        });
    }
}
