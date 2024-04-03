using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Sparcpoint.Extensions.Permissions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageAccountPermissionQuery : IAccountPermissionQuery
{
    private readonly BlobContainerClient _Client;
    private readonly string _Filename;

    public BlobStorageAccountPermissionQuery(BlobContainerClient client, string filename)
    {
        _Client = client;
        _Filename = filename;
    }

    public async IAsyncEnumerable<AccountPermissionEntry> RunAsync(PermissionQueryParameters parameters)
    {
        await _Client.EnsureCreatedAsync();

        List<Func<AccountPermissionEntry, bool>> filters = new();

        if (!string.IsNullOrWhiteSpace(parameters.AccountId))
            filters.Add((e) => e.AccountId == parameters.AccountId);

        // Key Filters
        if (!string.IsNullOrWhiteSpace(parameters.KeyExact))
        {
            filters.Add((e) => e.Entry.Key == parameters.KeyExact);
        } 
        else
        {
            if (!string.IsNullOrWhiteSpace(parameters.KeyStartsWith))
                filters.Add((e) => e.Entry.Key.StartsWith(parameters.KeyStartsWith));
            if (!string.IsNullOrWhiteSpace(parameters.KeyEndsWith))
                filters.Add((e) => e.Entry.Key.EndsWith(parameters.KeyEndsWith));
        }

        // Value Filters
        if (parameters.ValueExact != null)
            filters.Add((e) => e.Entry.Value == parameters.ValueExact);

        // Metadata Filters
        if (parameters.WithMetadata != null && parameters.WithMetadata.Count > 0)
        {
            filters.Add((e) =>
            {
                foreach(var kv in parameters.WithMetadata)
                {
                    Ensure.NotNullOrWhiteSpace(kv.Key);
                    if (e.Entry.Metadata == null || !e.Entry.Metadata.TryGetValue(kv.Key, out var value))
                        return false;

                    if (kv.Value == null)
                    {
                        if (value == null)
                            continue;

                        return false;
                    } else
                    {
                        if (!kv.Value.Equals(value))
                            return false;
                    }
                }

                return true;
            });
        }

        Func<AccountPermissionEntry, bool> filter = (e) =>
        {
            foreach (var f in filters)
            {
                if (!f(e))
                    return false;
            }

            return true;
        };

        await foreach(var scope in GetValidScopes(parameters.ScopeExact, parameters.ScopeStartsWith, parameters.ScopeEndsWith, parameters.ImmediateChildrenOnly))
        {
            var values = await GetValues(scope);
            foreach(var entry in values)
            {
                if (filter(entry))
                    yield return entry;
            }
        }
    }

    private async Task<IEnumerable<AccountPermissionEntry>> GetValues(ScopePath scope)
    {
        var bc = _Client.GetBlobClient(scope.Append(_Filename));
        var coll = await bc.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            return Array.Empty<AccountPermissionEntry>();

        return coll.Select(c => new AccountPermissionEntry(c.AccountId, scope, new PermissionEntry(c.Key, c.Value, c.Metadata))).ToArray();
    }

    private async IAsyncEnumerable<ScopePath> GetValidScopes(ScopePath? exact, ScopePath? startsWith, ScopePath? endsWith, bool immediateChildrenOnly)
    {
        if (exact != null)
        {
            if (await DoesScopeExistAsync(exact.Value))
            {
                yield return exact.Value;
                yield break;
            }    
        }

        Func<ScopePath, bool> check = (p) => true;
        if (endsWith != null)
            check = (p) => p.EndsWith(endsWith.Value);

        string? prefix = null;

        if (startsWith != null)
        {
            prefix = startsWith.Value;

            if (immediateChildrenOnly)
            {
                if (endsWith != null)
                {
                    check = (p) => p.Back() == startsWith && p.EndsWith(endsWith.Value);
                } else
                {
                    check = (p) => p.Back() == startsWith;
                }
            }
        }

        await foreach (var blob in _Client.GetBlobsAsync(states: BlobStates.None, prefix: prefix?.TrimStart('/')))
        {
            // Get the ScopePath (by removing the last segment which should be '.permissions')
            ScopePath foundScope = ScopePath.Parse(blob.Name).Back();
            if (check(foundScope))
                yield return foundScope;
        }
    }

    private async Task<bool> DoesScopeExistAsync(ScopePath scope)
    {
        var bc = _Client.GetBlobClient(scope.Append(_Filename));
        return await bc.ExistsAsync();
    }
}
