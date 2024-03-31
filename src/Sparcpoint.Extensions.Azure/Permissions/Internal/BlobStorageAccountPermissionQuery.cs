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

    public async Task<bool> HasAccessAsync(string accountId, string key, ScopePath? scope = null)
    {
        await _Client.CreateIfNotExistsAsync();

        Ensure.ArgumentNotNullOrWhiteSpace(accountId);
        Ensure.ArgumentNotNullOrWhiteSpace(key);

        var searchScope = scope ?? ScopePath.RootScope;
        do
        {
            var found = await GetScopeAsync(searchScope, accountId, key);
            if (found != null)
                return (found.Value == PermissionValue.Allow);
        } while ((searchScope = searchScope.Back()) != ScopePath.RootScope);

        return (await GetScopeAsync(ScopePath.RootScope, accountId, key))?.Value == PermissionValue.Allow;
    }

    private async Task<AccountPermissionEntryDto?> GetScopeAsync(ScopePath searchScope, string accountId, string key)
    {
        var bc = _Client.GetBlobClient(searchScope.Append(_Filename));
        if (await bc.ExistsAsync())
        {
            var coll = await bc.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
            if (coll != null)
            {
                var found = coll.FirstOrDefault(e => e.AccountId == accountId && e.Key == key);
                return found;
            }
        }

        return null;
    }

    public async IAsyncEnumerable<PermissionEntry> RunAsync(string accountId, PermissionQueryParameters parameters)
    {
        await _Client.CreateIfNotExistsAsync();

        Ensure.ArgumentNotNullOrWhiteSpace(accountId);

        List<Func<PermissionEntry, bool>> filters = new();
        
        // Key Filters
        if (!string.IsNullOrWhiteSpace(parameters.KeyExact))
        {
            filters.Add((e) => e.Key == parameters.KeyExact);
        } 
        else
        {
            if (!string.IsNullOrWhiteSpace(parameters.KeyStartsWith))
                filters.Add((e) => e.Key.StartsWith(parameters.KeyStartsWith));
            if (!string.IsNullOrWhiteSpace(parameters.KeyEndsWith))
                filters.Add((e) => e.Key.EndsWith(parameters.KeyEndsWith));
        }

        // Value Filters
        if (parameters.ValueExact != null)
            filters.Add((e) => e.Value == parameters.ValueExact);

        // Metadata Filters
        if (parameters.WithMetadata != null && parameters.WithMetadata.Count > 0)
        {
            filters.Add((e) =>
            {
                foreach(var kv in parameters.WithMetadata)
                {
                    Ensure.NotNullOrWhiteSpace(kv.Key);
                    if (e.Metadata == null || !e.Metadata.TryGetValue(kv.Key, out var value))
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

        Func<PermissionEntry, bool> filter = (e) =>
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
            await foreach(var entry in GetValues(scope, accountId))
            {
                if (filter(entry))
                    yield return entry;
            }
        }
    }

    private async IAsyncEnumerable<PermissionEntry> GetValues(ScopePath scope, string accountId)
    {
        var bc = _Client.GetBlobClient(scope.Append(_Filename));
        var coll = await bc.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
        if (coll == null)
            yield break;

        foreach(var entry in coll.Where(e => e.AccountId == accountId))
        {
            yield return entry.GetEntry(scope);
        }
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
                check = (p) => p.Back() == startsWith && check(p);
            }
        }

        await foreach (var blob in _Client.GetBlobsAsync(states: BlobStates.None, prefix: prefix))
        {
            // Get the ScopePath (by removing the last segment which should be 'permissions')
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
