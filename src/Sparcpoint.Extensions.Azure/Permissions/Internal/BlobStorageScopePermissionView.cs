﻿using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Sparcpoint.Extensions.Permissions;
using Sparcpoint.Extensions.Permissions.Extensions;

namespace Sparcpoint.Extensions.Azure.Permissions;

internal class BlobStorageScopePermissionView : IScopePermissionView
{
    private readonly BlobContainerClient _Client;
    private readonly BlobStoragePermissionStoreViewOptions _Options;
    private readonly string _Filename;

    // TODO: Allow for the inclusion of the root scope
    public BlobStorageScopePermissionView(BlobContainerClient client, ScopePath scope, BlobStoragePermissionStoreViewOptions options, string filename)
    {
        _Client = client;
        CurrentScope = scope;
        _Options = options;
        _Filename = filename;
    }

    public ScopePath CurrentScope { get; }

    public async IAsyncEnumerator<AccountPermissionEntry> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // 1. Load up all Entries
        List<AccountPermissionEntry> entries = new();
        await foreach(var scope in GetValidScopes())
        {
            var bc = _Client.GetBlobClient(scope.Append(_Filename));

            var values = await bc.GetAsJsonAsync<List<AccountPermissionEntryDto>>();
            if (values != null)
                entries.AddRange(values.Select(c => new AccountPermissionEntry(c.AccountId, c.GetEntry(scope))));
        }

        // 2. Calculate View
        var view = entries.CalculateView(CurrentScope);
        foreach (var e in view)
            yield return e;
    }

    private async IAsyncEnumerable<ScopePath> GetValidScopes()
    {
        var hierarchy = CurrentScope.GetHierarchy(includeRootScope: _Options.IncludeRootScopeInCalculations);
        if (CurrentScope == ScopePath.RootScope)
        {
            yield return ScopePath.RootScope;
            yield break;
        }

        foreach(var entry in hierarchy)
        {
            var bc = _Client.GetBlobClient(entry.Append(_Filename));
            if (await bc.ExistsAsync())
                yield return entry;
        }
    }
}