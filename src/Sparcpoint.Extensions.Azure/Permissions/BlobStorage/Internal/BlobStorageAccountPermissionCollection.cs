using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageAccountPermissionCollection : IAccountPermissionCollection
{
    private readonly BlobClient _Client;

    public BlobStorageAccountPermissionCollection(BlobContainerClient client, string accountId, ScopePath? scope, string filename)
    {
        AccountId = accountId;
        CurrentScope = scope ?? ScopePath.RootScope;

        _Client = client.GetBlobClient(CurrentScope.Append(filename));
    }

    public string AccountId { get; }
    public ScopePath CurrentScope { get; }

    public async Task ClearAsync()
    {
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

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            return false;
        
        return coll.Any(e => e.AccountId == AccountId && e.Key == key);
    }

    public async Task<PermissionEntry> GetAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            throw new KeyNotFoundException($"Entry {key} not found.");

        var found = coll.FirstOrDefault(e => e.AccountId == AccountId && e.Key == key);
        if (found == null)
            throw new KeyNotFoundException($"Entry {key} not found.");

        return new PermissionEntry(found.Key, found.Value, CurrentScope, found.Metadata);
    }

    public async IAsyncEnumerator<PermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var coll = await _Client.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            yield break;

        var accountItems = coll.Where(e => e.AccountId == AccountId).ToArray();
        foreach (var item in accountItems)
        {
            yield return new PermissionEntry(item.Key, item.Value, CurrentScope, item.Metadata);
        }
    }

    public async Task RemoveAsync(string key)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return null;

            var found = coll.Where(e => e.AccountId == AccountId && e.Key == key).ToArray();
            foreach(var f in found)
            {
                coll.Remove(f);
            }

            return coll;
        });
    }

    public async Task SetAsync(string key, PermissionValue value, Dictionary<string, string>? metadata = null)
    {
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        await _Client.UpdateAsJsonAsync<List<AccountPermissionEntryDto>>(async (coll) =>
        {
            if (coll == null)
                return new List<AccountPermissionEntryDto>();

            var found = coll.FirstOrDefault(e => e.AccountId == AccountId && e.Key == key);
            if (found == null)
            {
                found = new();
                coll.Add(found);
            }

            found.AccountId = AccountId;
            found.Key = key;
            found.Value = value;
            found.Metadata = metadata ?? new();

            return coll;
        });
    }
}
